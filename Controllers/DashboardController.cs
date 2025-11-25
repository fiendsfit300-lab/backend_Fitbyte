using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gym_FitByte.Data;

namespace Gym_FitByte.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;
        public DashboardController(AppDbContext context) => _context = context;

        // ============================================================
        // RESUMEN MENSUAL
        // ============================================================
        [HttpGet("resumen-mensual")]
        public async Task<IActionResult> ResumenMensual([FromQuery] int year)
        {
            if (year <= 0) year = DateTime.Now.Year;

            // ✅ Solo compras que YA impactaron inventario
            var compras = await _context.Compras
                .Include(c => c.Items)
                .Where(c =>
                    c.FechaCompra.Year == year &&
                    c.Items.Any() &&
                    c.Items.All(i => i.InventarioActualizado)
                )
                .GroupBy(c => c.FechaCompra.Month)
                .Select(g => new
                {
                    Mes = g.Key,
                    Total = g.Sum(x => x.Total)
                })
                .ToListAsync();

            var ventas = await _context.Ventas
                .Where(v => v.FechaVenta.Year == year)
                .GroupBy(v => v.FechaVenta.Month)
                .Select(g => new
                {
                    Mes = g.Key,
                    Total = g.Sum(x => x.Total)
                })
                .ToListAsync();

            return Ok(new { year, compras, ventas });
        }

        // ============================================================
        // RESUMEN DIARIO
        // ============================================================
        [HttpGet("resumen-diario")]
        public async Task<IActionResult> ResumenDiario([FromQuery] int year, [FromQuery] int month)
        {
            if (year <= 0) year = DateTime.Now.Year;
            if (month < 1 || month > 12) month = DateTime.Now.Month;

            // ✅ Solo compras confirmadas por inventario
            var compras = await _context.Compras
                .Include(c => c.Items)
                .Where(c =>
                    c.FechaCompra.Year == year &&
                    c.FechaCompra.Month == month &&
                    c.Items.Any() &&
                    c.Items.All(i => i.InventarioActualizado)
                )
                .GroupBy(c => c.FechaCompra.Day)
                .Select(g => new
                {
                    Dia = g.Key,
                    Total = g.Sum(x => x.Total)
                })
                .ToListAsync();

            var ventas = await _context.Ventas
                .Where(v => v.FechaVenta.Year == year && v.FechaVenta.Month == month)
                .GroupBy(v => v.FechaVenta.Day)
                .Select(g => new
                {
                    Dia = g.Key,
                    Total = g.Sum(x => x.Total)
                })
                .ToListAsync();

            return Ok(new { year, month, compras, ventas });
        }
    }
}
