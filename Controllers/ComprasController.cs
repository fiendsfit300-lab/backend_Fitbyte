using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gym_FitByte.Data;
using Gym_FitByte.Models;
using Gym_FitByte.DTOs;
using Gym_FitByte.Services;

namespace Gym_FitByte.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComprasController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IInventarioService _inventarioService;

        public ComprasController(AppDbContext context, IInventarioService inventarioService)
        {
            _context = context;
            _inventarioService = inventarioService;
        }

        // ============================================================
        // 🔥 CREAR COMPRA (Actualizado con nueva estructura de precios)
        // ============================================================
        [HttpPost("crear")]
        public async Task<IActionResult> Crear([FromBody] CrearCompraDto dto)
        {
            if (dto == null || dto.Items == null || dto.Items.Count == 0)
                return BadRequest("Debe incluir al menos un producto.");

            try
            {
                // IDs de productos involucrados
                var idsProductos = dto.Items.Select(i => i.ProductoId).Distinct().ToList();

                // Validar productos
                var productos = await _context.Productos
                    .Where(p => idsProductos.Contains(p.Id) && p.Activo)
                    .Include(p => p.Proveedor)
                    .ToListAsync();

                if (productos.Count != idsProductos.Count)
                    return BadRequest("Uno o más productos no existen o están inactivos.");

                // Crear compra principal
                var compra = new Compra
                {
                    FechaCompra = dto.FechaCompra,
                    Folio = dto.Folio,
                    Comentarios = dto.Comentarios
                };

                _context.Compras.Add(compra);
                await _context.SaveChangesAsync();

                // Procesar items
                foreach (var item in dto.Items)
                {
                    var prod = productos.First(p => p.Id == item.ProductoId);

                    // ⚠️ En compras, el precio enviado por el front es el precio del PAQUETE
                    var precioPaquete = item.PrecioUnitario;

                    // Subtotal
                    var subtotal = precioPaquete * item.Cantidad;

                    // Registrar item
                    compra.Items.Add(new CompraItem
                    {
                        CompraId = compra.Id,
                        ProductoId = prod.Id,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = precioPaquete, // precio paquete
                        Subtotal = subtotal,
                        InventarioActualizado = false
                    });

                    // ============================================================
                    // 🔥 ACTUALIZAR SOLO COSTO del producto
                    // - Precio (paquete)
                    // - PrecioUnitario (pieza)
                    // - NO tocamos PrecioFinal
                    // ============================================================

                    int piezas = prod.PiezasPorPaquete <= 0 ? 1 : prod.PiezasPorPaquete;

                    prod.Precio = precioPaquete;                  // costo paquete
                    prod.PrecioUnitario = precioPaquete / piezas; // costo por pieza
                }

                compra.Total = compra.Items.Sum(i => i.Subtotal);

                await _context.SaveChangesAsync();

                // Actualizar inventario
                await _inventarioService.ActualizarInventarioCompra(compra.Id);

                return Ok(new
                {
                    mensaje = "Compra registrada exitosamente.",
                    compra.Id,
                    compra.Total,
                    Items = compra.Items.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al crear la compra: {ex.Message}");
            }
        }

        // ============================================================
        // 🔍 LISTAR TODAS LAS COMPRAS
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var compras = await _context.Compras
                .Include(c => c.Items)
                    .ThenInclude(i => i.Producto)
                        .ThenInclude(p => p.Proveedor)
                .OrderByDescending(c => c.FechaCompra)
                .ToListAsync();

            var resultado = compras.Select(c => new
            {
                c.Id,
                c.FechaCompra,
                c.Folio,
                c.Comentarios,
                c.Total,

                Items = c.Items.Select(i => new
                {
                    i.ProductoId,
                    ProductoNombre = i.Producto!.Nombre,
                    i.Producto.Categoria,
                    Foto = i.Producto.FotoUrl,

                    Cantidad = i.Cantidad,

                    // precio del PAQUETE
                    PrecioPaquete = i.PrecioUnitario,

                    // costo por pieza
                    PrecioUnitarioPieza = i.PrecioUnitario /
                        (i.Producto.PiezasPorPaquete <= 0 ? 1 : i.Producto.PiezasPorPaquete),

                    i.Subtotal,

                    ProveedorId = i.Producto.ProveedorId,
                    Proveedor = i.Producto.Proveedor!.NombreEmpresa
                })
            });

            return Ok(resultado);
        }

        // ============================================================
        // 🔍 DETALLE DE UNA COMPRA
        // ============================================================
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Detalle(int id)
        {
            var compra = await _context.Compras
                .Include(c => c.Items)
                    .ThenInclude(i => i.Producto)
                        .ThenInclude(p => p.Proveedor)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (compra == null)
                return NotFound("Compra no encontrada.");

            return Ok(new
            {
                compra.Id,
                compra.FechaCompra,
                compra.Folio,
                compra.Comentarios,
                compra.Total,

                Items = compra.Items.Select(i => new
                {
                    i.ProductoId,
                    ProductoNombre = i.Producto!.Nombre,
                    i.Producto.Categoria,
                    Foto = i.Producto.FotoUrl,

                    Cantidad = i.Cantidad,

                    PrecioPaquete = i.PrecioUnitario,
                    PrecioUnitarioPieza = i.PrecioUnitario /
                        (i.Producto.PiezasPorPaquete <= 0 ? 1 : i.Producto.PiezasPorPaquete),

                    i.Subtotal,

                    ProveedorId = i.Producto.ProveedorId,
                    Proveedor = i.Producto.Proveedor!.NombreEmpresa
                })
            });
        }

        // ============================================================
        // 🔍 LISTAR COMPRAS POR PROVEEDOR
        // ============================================================
        [HttpGet("por-proveedor/{proveedorId:int}")]
        public async Task<IActionResult> PorProveedor(int proveedorId)
        {
            var compras = await _context.Compras
                .Include(c => c.Items)
                    .ThenInclude(i => i.Producto)
                .Where(c => c.Items.Any(x => x.Producto!.ProveedorId == proveedorId))
                .OrderByDescending(c => c.FechaCompra)
                .ToListAsync();

            var resultado = compras.Select(c => new
            {
                c.Id,
                c.FechaCompra,
                c.Folio,
                c.Comentarios,
                c.Total,

                Items = c.Items
                    .Where(i => i.Producto!.ProveedorId == proveedorId)
                    .Select(i => new
                    {
                        i.ProductoId,
                        ProductoNombre = i.Producto!.Nombre,
                        i.Producto.Categoria,
                        Foto = i.Producto.FotoUrl,

                        Cantidad = i.Cantidad,
                        PrecioPaquete = i.PrecioUnitario,
                        PrecioUnitarioPieza = i.PrecioUnitario /
                            (i.Producto.PiezasPorPaquete <= 0 ? 1 : i.Producto.PiezasPorPaquete),

                        i.Subtotal,
                        ProveedorId = proveedorId
                    })
            });

            return Ok(resultado);
        }
    }
}
