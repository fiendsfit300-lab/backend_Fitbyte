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
    public class VentasController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IInventarioService _inventarioService;

        public VentasController(AppDbContext context, IInventarioService inventarioService)
        {
            _context = context;
            _inventarioService = inventarioService;
        }

        // ============================================================
        // 🔥 FUNCIÓN: REGISTRAR MOVIMIENTO EN CORTE DE CAJA
        // ============================================================
        private async Task RegistrarMovimiento(string tipo, decimal monto, string descripcion)
        {
            // Busca el corte abierto actual
            var corte = await _context.CortesCaja.FirstOrDefaultAsync(c => c.Estado == 0);

            // Si no hay corte abierto, simplemente no registra nada
            if (corte == null)
                return;

            var mov = new MovimientoCaja
            {
                CorteCajaId = corte.Id,
                Tipo = tipo,
                Monto = monto,
                Descripcion = descripcion,
                Fecha = DateTime.Now
            };

            _context.MovimientosCaja.Add(mov);
            await _context.SaveChangesAsync();
        }

        // ============================================================
        // CREAR VENTA
        // ============================================================
        [HttpPost("crear")]
        public async Task<IActionResult> Crear([FromBody] CrearVentaDto dto)
        {
            if (dto.Items == null || dto.Items.Count == 0)
                return BadRequest("Debe incluir al menos un producto.");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var ids = dto.Items.Select(i => i.ProductoId).ToList();

                var productos = await _context.Productos
                    .Where(p => ids.Contains(p.Id) && p.Activo)
                    .ToListAsync();

                if (productos.Count != ids.Count)
                    return BadRequest("Uno o más productos no existen o están inactivos.");

                // Validar stock antes de crear la venta
                foreach (var item in dto.Items)
                {
                    var stock = await _inventarioService.ObtenerStockActual(item.ProductoId);
                    if (stock < item.Cantidad)
                    {
                        var p = productos.First(x => x.Id == item.ProductoId);
                        return BadRequest($"Stock insuficiente para '{p.Nombre}'. Disponible: {stock}, solicitado: {item.Cantidad}");
                    }
                }

                var venta = new Venta
                {
                    Cliente = dto.Cliente,
                    FechaVenta = dto.FechaVenta,
                    TipoVenta = dto.TipoVenta,
                    Completada = true
                };

                _context.Ventas.Add(venta);
                await _context.SaveChangesAsync();

                foreach (var item in dto.Items)
                {
                    var prod = productos.First(p => p.Id == item.ProductoId);

                    // 🔥 PRECIO DE VENTA POR PIEZA
                    // Si el front manda uno, se usa ese.
                    // Si NO, se usa el PrecioFinal del producto.
                    decimal precioVenta = item.PrecioUnitario > 0
                        ? item.PrecioUnitario
                        : prod.PrecioFinal;

                    decimal subtotal = precioVenta * item.Cantidad;

                    venta.Items.Add(new VentaItem
                    {
                        VentaId = venta.Id,
                        ProductoId = prod.Id,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = precioVenta,
                        Subtotal = subtotal
                    });
                }

                venta.Total = venta.Items.Sum(i => i.Subtotal);

                await _context.SaveChangesAsync();

                // Actualizar inventario
                await _inventarioService.ActualizarInventarioVenta(venta.Id);

                await transaction.CommitAsync();

                // 🔥 REGISTRAR MOVIMIENTO DE INGRESO EN EL CORTE
                await RegistrarMovimiento("Venta", venta.Total, $"Venta #{venta.Id}");

                return Ok(new
                {
                    mensaje = "Venta registrada exitosamente.",
                    venta.Id,
                    venta.Total,
                    items = venta.Items.Count
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Error al crear la venta: {ex.Message}");
            }
        }

        // ============================================================
        // LISTAR TODAS LAS VENTAS
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var ventas = await _context.Ventas
                .Include(v => v.Items)
                    .ThenInclude(i => i.Producto)
                .OrderByDescending(v => v.FechaVenta)
                .Select(v => new
                {
                    v.Id,
                    v.Cliente,
                    v.FechaVenta,
                    v.Total,
                    v.TipoVenta,
                    Items = v.Items.Select(i => new
                    {
                        i.ProductoId,
                        Nombre = i.Producto!.Nombre,
                        Foto = i.Producto!.FotoUrl,
                        Categoria = i.Producto!.Categoria,

                        Cantidad = i.Cantidad,

                        // ⭐ Este es el precio de venta (PrecioFinal o el que mandó el front)
                        PrecioUnitario = i.PrecioUnitario,

                        Subtotal = i.Subtotal
                    })
                })
                .ToListAsync();

            return Ok(ventas);
        }

        // ============================================================
        // DETALLE DE UNA VENTA POR ID
        // ============================================================
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Detalle(int id)
        {
            var venta = await _context.Ventas
                .Include(v => v.Items)
                    .ThenInclude(i => i.Producto)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venta == null)
                return NotFound("Venta no encontrada.");

            return Ok(new
            {
                venta.Id,
                venta.Cliente,
                venta.FechaVenta,
                venta.TipoVenta,
                venta.Total,
                Items = venta.Items.Select(i => new
                {
                    i.ProductoId,
                    Nombre = i.Producto!.Nombre,
                    Foto = i.Producto.FotoUrl,
                    Categoria = i.Producto!.Categoria,

                    Cantidad = i.Cantidad,

                    // ⭐ Precio de venta por pieza
                    PrecioUnitario = i.PrecioUnitario,

                    Subtotal = i.Subtotal
                })
            });
        }

        // ============================================================
        // BUSCAR VENTAS POR CLIENTE
        // ============================================================
        [HttpGet("por-cliente/{cliente}")]
        public async Task<IActionResult> PorCliente(string cliente)
        {
            var ventas = await _context.Ventas
                .Include(v => v.Items)
                    .ThenInclude(i => i.Producto)
                .Where(v => v.Cliente.ToLower().Contains(cliente.ToLower()))
                .OrderByDescending(v => v.FechaVenta)
                .ToListAsync();

            return Ok(ventas);
        }
    }
}
