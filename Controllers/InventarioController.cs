using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gym_FitByte.Data;
using Gym_FitByte.Models;
using Gym_FitByte.Services;

namespace Gym_FitByte.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventarioController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IInventarioService _inventarioService;

        public InventarioController(AppDbContext context, IInventarioService inventarioService)
        {
            _context = context;
            _inventarioService = inventarioService;
        }

        // ============================================================
        // INVENTARIO COMPLETO
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> ObtenerInventario()
        {
            var inventario = await _context.Inventario
                .Include(i => i.Producto)
                    .ThenInclude(p => p!.Proveedor)
                .OrderBy(i => i.Producto!.Nombre)
                .Select(i => new
                {
                    i.Id,
                    i.ProductoId,
                    ProductoNombre = i.Producto!.Nombre,
                    ProductoCategoria = i.Producto.Categoria,
                    ProductoPrecio = i.Producto.Precio,
                    ProductoFotoUrl = i.Producto.FotoUrl,
                    ProveedorId = i.Producto.ProveedorId,
                    ProveedorNombre = i.Producto.Proveedor!.NombreEmpresa,
                    StockActual = i.Cantidad,
                    i.CantidadComprada,
                    i.CantidadVendida,
                    i.FechaActualizacion,
                    ProductoActivo = i.Producto.Activo
                })
                .ToListAsync();

            return Ok(inventario);
        }

        // ============================================================
        // STOCK DE UN PRODUCTO
        // ============================================================
        [HttpGet("producto/{productoId:int}")]
        public async Task<IActionResult> ObtenerStockProducto(int productoId)
        {
            var stock = await _inventarioService.ObtenerStockActual(productoId);

            var producto = await _context.Productos
                .Include(p => p.Proveedor)
                .FirstOrDefaultAsync(p => p.Id == productoId);

            if (producto == null)
                return NotFound("Producto no encontrado.");

            return Ok(new
            {
                producto.Id,
                Nombre = producto.Nombre,
                Proveedor = producto.Proveedor!.NombreEmpresa,
                StockActual = stock,
                Precio = producto.Precio,
                Categoria = producto.Categoria,
                Foto = producto.FotoUrl,
                UltimaActualizacion = DateTime.Now
            });
        }

        // ============================================================
        // HISTORIAL DE MOVIMIENTOS
        // ============================================================
        [HttpGet("historial")]
        public async Task<IActionResult> ObtenerHistorial(
            [FromQuery] int? productoId = null,
            [FromQuery] TipoMovimiento? tipo = null,
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null)
        {
            var query = _context.HistorialMovimientos
                .Include(h => h.Producto)
                    .ThenInclude(p => p!.Proveedor)
                .AsQueryable();

            if (productoId.HasValue)
                query = query.Where(h => h.ProductoId == productoId.Value);

            if (tipo.HasValue)
                query = query.Where(h => h.Tipo == tipo.Value);

            if (fechaInicio.HasValue)
                query = query.Where(h => h.FechaMovimiento >= fechaInicio.Value);

            if (fechaFin.HasValue)
                query = query.Where(h => h.FechaMovimiento <= fechaFin.Value.AddDays(1));

            var historial = await query
                .OrderByDescending(h => h.FechaMovimiento)
                .Select(h => new
                {
                    h.Id,
                    h.ProductoId,
                    ProductoNombre = h.Producto!.Nombre,
                    Proveedor = h.Producto.Proveedor!.NombreEmpresa,
                    h.Tipo,
                    h.Cantidad,
                    h.Motivo,
                    h.CompraId,
                    h.VentaId,
                    h.PrecioUnitario,
                    h.Referencia,
                    h.FechaMovimiento
                })
                .ToListAsync();

            return Ok(historial);
        }

        // ============================================================
        // AJUSTAR INVENTARIO MANUALMENTE
        // ============================================================
        [HttpPost("ajustar")]
        public async Task<IActionResult> AjustarInventario([FromBody] AjusteInventarioDto dto)
        {
            try
            {
                var producto = await _context.Productos
                    .FirstOrDefaultAsync(p => p.Id == dto.ProductoId && p.Activo);

                if (producto == null)
                    return NotFound("Producto no encontrado o inactivo.");

                if (dto.NuevaCantidad < 0)
                    return BadRequest("La cantidad no puede ser negativa.");

                await _inventarioService.AjustarInventario(
                    dto.ProductoId,
                    dto.NuevaCantidad,
                    dto.Motivo,
                    dto.Referencia);

                return Ok(new { mensaje = "Inventario ajustado exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al ajustar inventario: {ex.Message}");
            }
        }

        // ============================================================
        // PRODUCTOS CON STOCK BAJO
        // ============================================================
        [HttpGet("stock-bajo")]
        public async Task<IActionResult> ObtenerStockBajo([FromQuery] int limite = 10)
        {
            var productos = await _context.Inventario
                .Include(i => i.Producto)
                    .ThenInclude(p => p!.Proveedor)
                .Where(i => i.Cantidad <= limite && i.Producto!.Activo)
                .OrderBy(i => i.Cantidad)
                .Select(i => new
                {
                    i.ProductoId,
                    ProductoNombre = i.Producto!.Nombre,
                    Proveedor = i.Producto.Proveedor!.NombreEmpresa,
                    StockActual = i.Cantidad,
                    StockMinimo = limite,
                    i.FechaActualizacion
                })
                .ToListAsync();

            return Ok(productos);
        }

        // ============================================================
        // REPORTE GENERAL DE INVENTARIO
        // ============================================================
        [HttpGet("reporte")]
        public async Task<IActionResult> ObtenerReporte()
        {
            var productosActivos = await _context.Productos.CountAsync(p => p.Activo);

            var totalPiezas = await _context.Inventario.SumAsync(i => i.Cantidad);

            var valorTotal = await _context.Inventario
                .Include(i => i.Producto)
                .Where(i => i.Producto!.Activo)
                .SumAsync(i => i.Cantidad * i.Producto.Precio);

            var movimientosHoy = await _context.HistorialMovimientos
                .Where(h => h.FechaMovimiento.Date == DateTime.Today)
                .CountAsync();

            var sinStock = await _context.Inventario
                .Include(i => i.Producto)
                .Where(i => i.Cantidad == 0 && i.Producto!.Activo)
                .CountAsync();

            return Ok(new
            {
                ProductosActivos = productosActivos,
                TotalPiezas = totalPiezas,
                ValorTotal = valorTotal,
                MovimientosHoy = movimientosHoy,
                ProductosSinStock = sinStock,
                FechaReporte = DateTime.Now
            });
        }
    }

    // DTO para ajuste manual
    public class AjusteInventarioDto
    {
        public int ProductoId { get; set; }
        public int NuevaCantidad { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public string? Referencia { get; set; }
    }
}
