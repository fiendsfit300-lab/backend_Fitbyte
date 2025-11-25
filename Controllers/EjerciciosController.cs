using Gym_FitByte.Data;
using Gym_FitByte.DTOs;
using Gym_FitByte.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gym_FitByte.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EjerciciosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EjerciciosController(AppDbContext context)
        {
            _context = context;
        }

        // ============================
        // CREAR EJERCICIO
        // ============================
        [HttpPost("crear")]
        public async Task<IActionResult> CrearEjercicio([FromBody] CrearEjercicioDto dto)
        {
            var rutina = await _context.Rutinas.FindAsync(dto.RutinaId);

            if (rutina == null)
                return BadRequest("La rutina seleccionada no existe.");

            var ejercicio = new Ejercicio
            {
                Nombre = dto.Nombre,
                Series = dto.Series,
                Repeticiones = dto.Repeticiones,
                Descanso = dto.Descanso,
                Notas = dto.Notas,
                RutinaId = dto.RutinaId
            };

            _context.Ejercicios.Add(ejercicio);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Ejercicio creado correctamente.",
                ejercicio
            });
        }

        // ============================
        // ACTUALIZAR EJERCICIO
        // ============================
        [HttpPut("actualizar/{id:int}")]
        public async Task<IActionResult> ActualizarEjercicio(int id, [FromBody] CrearEjercicioDto dto)
        {
            var ejercicio = await _context.Ejercicios.FindAsync(id);

            if (ejercicio == null)
                return NotFound("El ejercicio no existe.");

            ejercicio.Nombre = dto.Nombre;
            ejercicio.Series = dto.Series;
            ejercicio.Repeticiones = dto.Repeticiones;
            ejercicio.Descanso = dto.Descanso;
            ejercicio.Notas = dto.Notas;

            await _context.SaveChangesAsync();

            return Ok("Ejercicio actualizado correctamente.");
        }

        // ============================
        // LISTAR EJERCICIOS DE UNA RUTINA
        // ============================
        [HttpGet("por-rutina/{rutinaId:int}")]
        public async Task<IActionResult> ObtenerEjerciciosPorRutina(int rutinaId)
        {
            var ejercicios = await _context.Ejercicios
                .Where(e => e.RutinaId == rutinaId)
                .ToListAsync();

            return Ok(ejercicios);
        }

        // ============================
        // DETAILS
        // ============================
        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerEjercicio(int id)
        {
            var ejercicio = await _context.Ejercicios.FindAsync(id);

            if (ejercicio == null)
                return NotFound();

            return Ok(ejercicio);
        }

        // ============================
        // DELETE
        // ============================
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> EliminarEjercicio(int id)
        {
            var ejercicio = await _context.Ejercicios.FindAsync(id);

            if (ejercicio == null)
                return NotFound();

            _context.Ejercicios.Remove(ejercicio);
            await _context.SaveChangesAsync();

            return Ok("Ejercicio eliminado.");
        }
    }
}
