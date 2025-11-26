using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Gym_FitByte.Data;
using Gym_FitByte.Models;
using Gym_FitByte.DTOs;

namespace Gym_FitByte.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private const string ContainerName = "fotos";

        public ProductosController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // ============================================================
        // REGISTRAR PRODUCTO
        // ============================================================
        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar([FromForm] CrearProductoDto dto)
        {
            if (dto.ProveedorId <= 0)
                return BadRequest("ProveedorId inválido.");

            var proveedor = await _context.Proveedores.FindAsync(dto.ProveedorId);
            if (proveedor == null || !proveedor.Activo)
                return BadRequest("Proveedor inválido o inactivo.");

            bool existe = await _context.Productos
                .AnyAsync(p => p.ProveedorId == dto.ProveedorId &&
                               p.Nombre.ToLower() == dto.Nombre.ToLower());

            if (existe)
                return Conflict("Ese producto ya existe para este proveedor.");

            string? fotoUrl = null;

            if (dto.Foto != null && dto.Foto.Length > 0)
                fotoUrl = await SubirFotoABlob(dto.Foto);

            // Aseguramos piezas mínimas
            int piezas = dto.PiezasPorPaquete <= 0 ? 1 : dto.PiezasPorPaquete;

            // Costo por pieza
            decimal precioUnitario = piezas > 0 ? dto.Precio / piezas : dto.Precio;

            // Precio final de venta por pieza:
            // si no mandan nada o es 0, por default igual al costo (ya luego lo actualizan)
            decimal precioFinal = dto.PrecioFinal > 0 ? dto.PrecioFinal : precioUnitario;

            var prod = new Producto
            {
                ProveedorId = dto.ProveedorId,
                Nombre = dto.Nombre,
                Precio = dto.Precio,                  // paquete (costo)
                PrecioUnitario = precioUnitario,      // costo por pieza
                PrecioFinal = precioFinal,            // venta por pieza
                Categoria = dto.Categoria,
                FotoUrl = fotoUrl,
                Activo = true,
                PiezasPorPaquete = piezas
            };

            _context.Productos.Add(prod);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Producto registrado.", id = prod.Id });
        }

        // ============================================================
        // REGISTRAR VARIOS PRODUCTOS
        // ============================================================
        [HttpPost("registrar-multiples")]
        public async Task<IActionResult> RegistrarMultiples([FromForm] CrearProductosMultiplesDto dto)
        {
            if (dto.ProveedorId <= 0)
                return BadRequest("ProveedorId inválido.");

            var proveedor = await _context.Proveedores.FindAsync(dto.ProveedorId);
            if (proveedor == null || !proveedor.Activo)
                return BadRequest("Proveedor inválido o inactivo.");

            if (!dto.Nombre.Any())
                return BadRequest("Debe enviar al menos un producto.");

            var nuevos = new List<Producto>();
            var errores = new List<string>();

            for (int i = 0; i < dto.Nombre.Count; i++)
            {
                var nombre = dto.Nombre[i];

                bool existe = await _context.Productos
                    .AnyAsync(p => p.ProveedorId == dto.ProveedorId &&
                                   p.Nombre.ToLower() == nombre.ToLower());

                if (existe)
                {
                    errores.Add($"El producto '{nombre}' ya existe.");
                    continue;
                }

                string? fotoUrl = null;
                var foto = dto.Foto.ElementAtOrDefault(i);

                if (foto != null && foto.Length > 0)
                    fotoUrl = await SubirFotoABlob(foto);

                int piezas = dto.PiezasPorPaquete.ElementAtOrDefault(i) <= 0
                    ? 1
                    : dto.PiezasPorPaquete[i];

                decimal precioPaquete = dto.Precio.ElementAtOrDefault(i);
                decimal precioUnitario = piezas > 0 ? precioPaquete / piezas : precioPaquete;

                decimal precioFinal = 0;
                if (dto.PrecioFinal != null && dto.PrecioFinal.Count > i)
                {
                    precioFinal = dto.PrecioFinal[i];
                }
                if (precioFinal <= 0)
                    precioFinal = precioUnitario;

                nuevos.Add(new Producto
                {
                    ProveedorId = dto.ProveedorId,
                    Nombre = nombre,
                    Precio = precioPaquete,
                    PrecioUnitario = precioUnitario,
                    PrecioFinal = precioFinal,
                    Categoria = dto.Categoria.ElementAtOrDefault(i) ?? string.Empty,
                    FotoUrl = fotoUrl,
                    Activo = true,
                    PiezasPorPaquete = piezas
                });
            }

            _context.Productos.AddRange(nuevos);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Productos registrados.",
                registrados = nuevos.Count,
                errores
            });
        }

        // ============================================================
        // ACTUALIZAR PRODUCTO
        // ============================================================
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromForm] ActualizarProductoDto dto)
        {
            var prod = await _context.Productos.FindAsync(id);

            if (prod == null)
                return NotFound("Producto no encontrado.");

            if (!string.Equals(prod.Nombre, dto.Nombre, StringComparison.OrdinalIgnoreCase))
            {
                bool existe = await _context.Productos
                    .AnyAsync(p => p.ProveedorId == prod.ProveedorId &&
                                   p.Nombre.ToLower() == dto.Nombre.ToLower());

                if (existe)
                    return Conflict("Ya existe un producto con ese nombre.");
            }

            if (dto.Foto != null && dto.Foto.Length > 0)
                prod.FotoUrl = await SubirFotoABlob(dto.Foto);

            prod.Nombre = dto.Nombre;

            // Precio del paquete (costo)
            prod.Precio = dto.Precio;

            // Recalcular costo por pieza
            int piezas = dto.PiezasPorPaquete <= 0 ? 1 : dto.PiezasPorPaquete;
            prod.PiezasPorPaquete = piezas;
            prod.PrecioUnitario = piezas > 0 ? dto.Precio / piezas : dto.Precio;

            // Actualizar precio final de venta (si mandan algo)
            if (dto.PrecioFinal > 0)
                prod.PrecioFinal = dto.PrecioFinal;

            prod.Categoria = dto.Categoria;
            prod.Activo = dto.Activo;

            await _context.SaveChangesAsync();
            return Ok(new { mensaje = "Producto actualizado." });
        }

        // ============================================================
        // DESACTIVAR PRODUCTO
        // ============================================================
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var prod = await _context.Productos.FindAsync(id);
            if (prod == null) return NotFound("Producto no encontrado.");

            prod.Activo = false;
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Producto desactivado." });
        }

        // ============================================================
        // LISTAR PRODUCTOS POR PROVEEDOR
        // ============================================================
        [HttpGet("por-proveedor/{proveedorId:int}")]
        public async Task<IActionResult> PorProveedor(int proveedorId)
        {
            var productos = await _context.Productos
                .Where(p => p.ProveedorId == proveedorId && p.Activo)
                .OrderBy(p => p.Nombre)
                .Select(p => new
                {
                    p.Id,
                    p.ProveedorId,
                    p.Nombre,
                    Precio = p.Precio,              // paquete (costo)
                    PrecioUnitario = p.PrecioUnitario,
                    PrecioFinal = p.PrecioFinal,    // venta
                    p.Categoria,
                    p.FotoUrl,
                    p.Activo,
                    p.PiezasPorPaquete
                })
                .ToListAsync();

            return Ok(productos);
        }

        // ============================================================
        // PRODUCTOS DISPONIBLES PARA VENTA (INVENTARIO)
        // ============================================================
        [HttpGet("disponibles")]
        public async Task<IActionResult> Disponibles()
        {
            var productos = await _context.Inventario
                .Include(i => i.Producto)
                .Where(i => i.Cantidad > 0 && i.Producto!.Activo)
                .Select(i => new
                {
                    ProductoId = i.ProductoId,
                    Nombre = i.Producto!.Nombre,

                    // Costo del paquete
                    PrecioPaquete = i.Producto.Precio,

                    // 🔹 Costo por pieza (si algún día lo quieres usar en el front)
                    PrecioCostoUnitario = i.Producto.PrecioUnitario,

                    // 🔥 Precio de venta por pieza (ESTE usará tu módulo de ventas)
                    PrecioUnitario = i.Producto.PrecioFinal,

                    i.Producto.Categoria,
                    i.Producto.FotoUrl,
                    StockActual = i.Cantidad
                })
                .OrderBy(i => i.Nombre)
                .ToListAsync();

            return Ok(productos);
        }

        // ============================================================
        // SUBIR IMAGEN A BLOB
        // ============================================================
        private async Task<string> SubirFotoABlob(IFormFile archivo)
        {
            var cs = _config.GetConnectionString("AzureBlobStorage");
            var service = new BlobServiceClient(cs);
            var container = service.GetBlobContainerClient(ContainerName);

            await container.CreateIfNotExistsAsync();
            await container.SetAccessPolicyAsync(PublicAccessType.Blob);

            var nombre = $"{Guid.NewGuid()}{Path.GetExtension(archivo.FileName)}";
            var blob = container.GetBlobClient(nombre);

            using var stream = archivo.OpenReadStream();
            await blob.UploadAsync(stream, overwrite: true);

            return blob.Uri.ToString();
        }
    }
}
