using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gym_FitByte.Data;
using Gym_FitByte.Models;

namespace Gym_FitByte.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PreRegistrosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PreRegistrosController(AppDbContext context)
        {
            _context = context;
        }

        // ========== GET: api/PreRegistros ==========
        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var hoy = DateTime.Now;

            // ✅ Traemos los registros y luego hacemos la lógica en memoria
            var preRegistros = await _context.PreRegistros
                .AsNoTracking()
                .ToListAsync();

            // ✅ Evaluamos la fecha de expiración en memoria (ya fuera del IQueryable)
            foreach (var p in preRegistros)
            {
                var fechaExpiracion = p.FechaRegistro.AddDays(3);
                if (p.Estado == EstadoPreRegistro.Pendiente && fechaExpiracion < hoy)
                {
                    p.Estado = EstadoPreRegistro.Vencido;
                }
            }

            return Ok(preRegistros);
        }

        // ========== POST: api/PreRegistros/crear ==========
        [HttpPost("crear")]
        public async Task<IActionResult> Crear([FromBody] PreRegistro nuevo)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            nuevo.FechaRegistro = DateTime.Now;
            nuevo.Estado = EstadoPreRegistro.Pendiente;

            _context.PreRegistros.Add(nuevo);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Pre-registro enviado con éxito.", nuevo.Id });
        }

        // ========== PUT: api/PreRegistros/aceptar/{id} ==========
        // ========== PUT: api/PreRegistros/aceptar/{id} ==========
        [HttpPut("aceptar/{id:int}")]
        public async Task<IActionResult> Aceptar(int id)
        {
            var pre = await _context.PreRegistros.FindAsync(id);
            if (pre == null)
                return NotFound("Pre-registro no encontrado.");

            if (pre.Estado != EstadoPreRegistro.Pendiente)
                return BadRequest("El pre-registro ya fue procesado.");

            // ✅ SOLO marcar como aceptado
            pre.Estado = EstadoPreRegistro.Aceptado;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Pre-registro aceptado correctamente.",
                pre.Id,
                pre.Nombre,
                pre.Correo,
                pre.Telefono
            });
        }

        // ========== PUT: api/PreRegistros/rechazar/{id} ==========
        [HttpPut("rechazar/{id:int}")]
        public async Task<IActionResult> Rechazar(int id)
        {
            var pre = await _context.PreRegistros.FindAsync(id);
            if (pre == null)
                return NotFound("Pre-registro no encontrado.");

            if (pre.Estado != EstadoPreRegistro.Pendiente)
                return BadRequest("El pre-registro ya fue procesado.");

            pre.Estado = EstadoPreRegistro.Rechazado;
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Pre-registro rechazado." });
        }
    }
}
