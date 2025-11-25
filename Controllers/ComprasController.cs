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
        // CREAR COMPRA
        // ============================================================
        [HttpPost("crear")]
        public async Task<IActionResult> Crear([FromBody] CrearCompraDto dto)
        {
            if (dto == null || dto.Items == null || dto.Items.Count == 0)
                return BadRequest("Debe incluir al menos un producto.");

            // NO usar transaction aquí. El inventario service usa su propia transacción.
            try
            {
                // Validar existencia de productos
                var idsProductos = dto.Items.Select(i => i.ProductoId).Distinct().ToList();

                var productos = await _context.Productos
                    .Where(p => idsProductos.Contains(p.Id) && p.Activo)
                    .Include(p => p.Proveedor)
                    .ToListAsync();

                if (productos.Count != idsProductos.Count)
                    return BadRequest("Uno o más productos no existen o están inactivos.");

                // Crear compra
                var compra = new Compra
                {
                    FechaCompra = dto.FechaCompra,
                    Folio = dto.Folio,
                    Comentarios = dto.Comentarios
                };

                _context.Compras.Add(compra);
                await _context.SaveChangesAsync();

                // Crear items
                foreach (var item in dto.Items)
                {
                    var prod = productos.First(p => p.Id == item.ProductoId);

                    var subtotal = item.Cantidad * item.PrecioUnitario;

                    compra.Items.Add(new CompraItem
                    {
                        CompraId = compra.Id,
                        ProductoId = prod.Id,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = item.PrecioUnitario,
                        Subtotal = subtotal,
                        InventarioActualizado = false
                    });
                }

                // Calcular TOTAL
                compra.Total = compra.Items.Sum(i => i.Subtotal);

                await _context.SaveChangesAsync();

                // Actualizar inventario (usa su propia transacción interna)
                await _inventarioService.ActualizarInventarioCompra(compra.Id);

                return Ok(new
                {
                    mensaje = "Compra registrada exitosamente.",
                    compra.Id,
                    Total = compra.Total,
                    Items = compra.Items.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al crear la compra: {ex.Message}");
            }
        }

        // ============================================================
        // LISTAR COMPRAS
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
                    Nombre = i.Producto!.Nombre,
                    Categoria = i.Producto!.Categoria,
                    Foto = i.Producto!.FotoUrl,
                    i.Cantidad,
                    Precio = i.PrecioUnitario,
                    i.Subtotal,
                    ProveedorId = i.Producto!.ProveedorId,
                    Proveedor = i.Producto!.Proveedor!.NombreEmpresa
                })
            });

            return Ok(resultado);
        }

        // ============================================================
        // DETALLE COMPRA
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
                    Nombre = i.Producto!.Nombre,
                    Categoria = i.Producto!.Categoria,
                    Foto = i.Producto!.FotoUrl,
                    i.Cantidad,
                    i.PrecioUnitario,
                    i.Subtotal,
                    ProveedorId = i.Producto!.ProveedorId,
                    Proveedor = i.Producto!.Proveedor!.NombreEmpresa
                })
            });
        }

        // ============================================================
        // LISTAR POR PROVEEDOR
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
                        Nombre = i.Producto!.Nombre,
                        Categoria = i.Producto!.Categoria,
                        Foto = i.Producto!.FotoUrl,
                        i.Cantidad,
                        i.PrecioUnitario,
                        i.Subtotal,
                        ProveedorId = proveedorId
                    })
            });

            return Ok(resultado);
        }
    }
}
