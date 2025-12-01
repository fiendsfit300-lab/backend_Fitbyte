using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gym_FitByte.Data;
using Gym_FitByte.Models;

namespace Gym_FitByte.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VentaVisitaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VentaVisitaController(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // 🔥 FUNCIÓN: REGISTRAR MOVIMIENTO EN CORTE
        // ============================================================
        private async Task RegistrarMovimiento(string tipo, decimal monto, string descripcion)
        {
            var corte = await _context.CortesCaja.FirstOrDefaultAsync(c => c.Estado == 0);

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
        // 🔥 REGISTRAR UNA VENTA DE VISITA
        // ============================================================
        [HttpPost("registrar")]
        public async Task<IActionResult> RegistrarVenta([FromBody] VentaVisita visita)
        {
            if (visita.Costo <= 0)
                return BadRequest("El costo de la visita debe ser mayor a 0.");

            if (string.IsNullOrEmpty(visita.NombreCliente))
                return BadRequest("Debe ingresar el nombre del cliente.");

            _context.VentasVisitas.Add(visita);
            await _context.SaveChangesAsync();

            // 🔥 REGISTRAR MOVIMIENTO EN CORTE
            await RegistrarMovimiento("Visita", visita.Costo, $"Venta de visita #{visita.Id}");

            return Ok(new
            {
                mensaje = "Venta de visita registrada correctamente.",
                visita.Id,
                visita.NombreCliente,
                visita.Costo,
                visita.FechaVenta
            });
        }

        // ============================================================
        // 🔍 OBTENER TODAS LAS VISITAS
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> ObtenerVentas()
        {
            var ventas = await _context.VentasVisitas
                .OrderByDescending(v => v.FechaVenta)
                .ToListAsync();

            return Ok(ventas);
        }

        // ============================================================
        // 🔍 OBTENER VISITA POR ID
        // ============================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerVentaPorId(int id)
        {
            var venta = await _context.VentasVisitas.FindAsync(id);

            if (venta == null)
                return NotFound("Venta no encontrada.");

            return Ok(venta);
        }

        // ============================================================
        // 🔍 VISITAS DE HOY
        // ============================================================
        [HttpGet("hoy")]
        public async Task<IActionResult> VentasDeHoy()
        {
            var hoy = DateTime.Today;

            var ventas = await _context.VentasVisitas
                .Where(v => v.FechaVenta.Date == hoy)
                .ToListAsync();

            return Ok(new
            {
                total = ventas.Count,
                ventas
            });
        }

        // ============================================================
        // 🔍 VISITAS DE AYER
        // ============================================================
        [HttpGet("ayer")]
        public async Task<IActionResult> VentasDeAyer()
        {
            var ayer = DateTime.Today.AddDays(-1);

            var ventas = await _context.VentasVisitas
                .Where(v => v.FechaVenta.Date == ayer)
                .ToListAsync();

            return Ok(new
            {
                total = ventas.Count,
                ventas
            });
        }

        // ============================================================
        // 🔍 VISITAS DE LA SEMANA
        // ============================================================
        [HttpGet("semana")]
        public async Task<IActionResult> VentasDeSemana()
        {
            var hoy = DateTime.Today;
            var primerDia = hoy.AddDays(-(int)hoy.DayOfWeek + 1);
            var ultimoDia = primerDia.AddDays(6);

            var ventas = await _context.VentasVisitas
                .Where(v => v.FechaVenta.Date >= primerDia && v.FechaVenta.Date <= ultimoDia)
                .ToListAsync();

            return Ok(new
            {
                total = ventas.Count,
                ventas
            });
        }

        // ============================================================
        // 🔍 VISITAS DEL MES
        // ============================================================
        [HttpGet("mes")]
        public async Task<IActionResult> VentasDeMes()
        {
            var hoy = DateTime.Today;

            var primerDia = new DateTime(hoy.Year, hoy.Month, 1);
            var ultimoDia = primerDia.AddMonths(1).AddDays(-1);

            var ventas = await _context.VentasVisitas
                .Where(v => v.FechaVenta.Date >= primerDia && v.FechaVenta.Date <= ultimoDia)
                .ToListAsync();

            return Ok(new
            {
                total = ventas.Count,
                ventas
            });
        }
    }
}
