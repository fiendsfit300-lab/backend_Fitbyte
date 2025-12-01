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
        // 1. 🔥 ABRIR CORTE DE CAJA
        // ============================================================
        [HttpPost("abrir")]
        public async Task<IActionResult> Abrir([FromBody] decimal montoInicial)
        {
            // Revisar si existe un corte abierto
            var abierto = await _context.CortesCaja
                .FirstOrDefaultAsync(c => c.Estado == 0);

            if (abierto != null)
            {
                return BadRequest(new { mensaje = "Ya hay un corte de caja abierto." });
            }

            var corte = new CorteCaja
            {
                FechaApertura = DateTime.Now,
                MontoInicial = montoInicial,
                Estado = 0 // abierto
            };

            _context.CortesCaja.Add(corte);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Corte de caja abierto correctamente.",
                corte.Id,
                corte.MontoInicial,
                corte.FechaApertura
            });
        }

        // ============================================================
        // 2. 🔥 REGISTRAR MOVIMIENTO MANUAL (ya no lo usas desde front)
        // ============================================================
        [HttpPost("movimiento")]
        public async Task<IActionResult> RegistrarMovimiento([FromBody] MovimientoCaja dto)
        {
            var corte = await _context.CortesCaja.FirstOrDefaultAsync(c => c.Estado == 0);

            if (corte == null)
                return BadRequest(new { mensaje = "No hay corte abierto para registrar movimiento." });

            dto.CorteCajaId = corte.Id;
            dto.Fecha = DateTime.Now;

            _context.MovimientosCaja.Add(dto);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Movimiento registrado correctamente." });
        }

        // ============================================================
        // 3. 🔥 CERRAR CORTE DE CAJA
        // ============================================================
        [HttpPost("cerrar")]
        public async Task<IActionResult> Cerrar()
        {
            var corte = await _context.CortesCaja
                .Include(c => c.Movimientos)
                .FirstOrDefaultAsync(c => c.Estado == 0);

            if (corte == null)
                return BadRequest(new { mensaje = "No hay corte abierto." });

            // Calcular total de movimientos
            var totalMovimientos = corte.Movimientos.Sum(m => m.Monto);

            corte.MontoFinal = corte.MontoInicial + totalMovimientos;
            corte.FechaCierre = DateTime.Now;
            corte.Estado = 1; // cerrado

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Corte cerrado correctamente.",
                corte.Id,
                corte.MontoInicial,
                TotalMovimientos = totalMovimientos,
                corte.MontoFinal,
                corte.FechaCierre
            });
        }

        // ============================================================
        // 4. 🔍 OBTENER CORTE POR ID (incluye movimientos)
        // ============================================================
        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerCorte(int id)
        {
            var corte = await _context.CortesCaja
                .Include(c => c.Movimientos)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (corte == null)
                return NotFound(new { mensaje = "Corte no encontrado." });

            return Ok(corte);
        }

        // ============================================================
        // 5. 🔍 CORTES DE UN DÍA (incluye movimientos)
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
        // 6. 🔍 CORTES DEL MES (incluye movimientos)
        // ============================================================
        [HttpGet("historial/mes")]
        public async Task<IActionResult> CortesMes([FromQuery] int year, [FromQuery] int month)
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
