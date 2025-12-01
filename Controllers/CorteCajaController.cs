using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gym_FitByte.Data;
using Gym_FitByte.Models;

namespace Gym_FitByte.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CorteCajaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CorteCajaController(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // 1. ABRIR CORTE DE CAJA
        // ============================================================
        [HttpPost("abrir")]
        public async Task<IActionResult> Abrir([FromBody] decimal montoInicial)
        {
            // verificar si ya hay un corte abierto
            var abierto = await _context.CortesCaja
                .FirstOrDefaultAsync(c => c.Estado == 0);

            if (abierto != null)
            {
                return BadRequest(new { mensaje = "Ya hay un corte abierto" });
            }

            var corte = new CorteCaja
            {
                FechaApertura = DateTime.Now,
                MontoInicial = montoInicial,
                Estado = 0
            };

            _context.CortesCaja.Add(corte);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Corte abierto", corte.Id });
        }

        // ============================================================
        // 2. AGREGAR MOVIMIENTO (VENTA, VISITA, COMPRA…)
        // ============================================================
        [HttpPost("movimiento")]
        public async Task<IActionResult> RegistrarMovimiento([FromBody] MovimientoCaja dto)
        {
            var corte = await _context.CortesCaja
                .FirstOrDefaultAsync(c => c.Estado == 0);

            if (corte == null)
                return BadRequest(new { mensaje = "No hay corte de caja abierto" });

            dto.CorteCajaId = corte.Id;
            dto.Fecha = DateTime.Now;

            _context.MovimientosCaja.Add(dto);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Movimiento registrado" });
        }

        // ============================================================
        // 3. CERRAR CORTE
        // ============================================================
        [HttpPost("cerrar")]
        public async Task<IActionResult> Cerrar()
        {
            var corte = await _context.CortesCaja
                .Include(c => c.Movimientos)
                .FirstOrDefaultAsync(c => c.Estado == 0);

            if (corte == null)
                return BadRequest(new { mensaje = "No hay corte abierto" });

            // Calcular total del corte
            var totalMovimientos = corte.Movimientos.Sum(m => m.Monto);

            corte.MontoFinal = corte.MontoInicial + totalMovimientos;
            corte.FechaCierre = DateTime.Now;
            corte.Estado = 1;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Corte cerrado correctamente",
                corte.Id,
                corte.MontoFinal,
                totalMovimientos
            });
        }

        // ============================================================
        // 4. CONSULTAR UN CORTE ESPECÍFICO
        // ============================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerCorte(int id)
        {
            var corte = await _context.CortesCaja
                .Include(c => c.Movimientos)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (corte == null)
                return NotFound();

            return Ok(corte);
        }

        // ============================================================
        // 5. CORTES POR DÍA
        // ============================================================
        [HttpGet("historial/dia")]
        public async Task<IActionResult> CortesPorDia([FromQuery] DateTime fecha)
        {
            var res = await _context.CortesCaja
                .Where(c => c.FechaApertura.Date == fecha.Date)
                .Include(c => c.Movimientos)
                .OrderBy(c => c.FechaApertura)
                .ToListAsync();

            return Ok(res);
        }

        // ============================================================
        // 6. HISTORIAL MENSUAL
        // ============================================================
        [HttpGet("historial/mes")]
        public async Task<IActionResult> CortesMes(int year, int month)
        {
            var res = await _context.CortesCaja
                .Where(c => c.FechaApertura.Year == year && c.FechaApertura.Month == month)
                .Include(c => c.Movimientos)
                .OrderBy(c => c.FechaApertura)
                .ToListAsync();

            return Ok(res);
        }
    }
}
