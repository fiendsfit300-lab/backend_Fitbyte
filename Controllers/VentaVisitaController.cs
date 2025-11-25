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


        [HttpPost("registrar")]
        public async Task<IActionResult> RegistrarVenta([FromBody] VentaVisita visita)
        {
            if (visita.Costo <= 0)
                return BadRequest("El costo de la visita debe ser mayor a 0.");

            if (string.IsNullOrEmpty(visita.NombreCliente))
                return BadRequest("Debe ingresar el nombre del cliente.");

            _context.VentasVisitas.Add(visita);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Venta de visita registrada correctamente.",
                visita.Id,
                visita.NombreCliente,
                visita.Costo,
                visita.FechaVenta
            });
        }


        [HttpGet]
        public async Task<IActionResult> ObtenerVentas()
        {
            var ventas = await _context.VentasVisitas
                .OrderByDescending(v => v.FechaVenta)
                .ToListAsync();

            return Ok(ventas);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerVentaPorId(int id)
        {
            var venta = await _context.VentasVisitas.FindAsync(id);
            if (venta == null)
                return NotFound("Venta no encontrada.");

            return Ok(venta);
        }


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
