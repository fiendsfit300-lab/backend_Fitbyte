using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gym_FitByte.Data;
using Gym_FitByte.Models;
using Gym_FitByte.DTOs;

namespace Gym_FitByte.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VisitasController : ControllerBase
    {
        private readonly AppDbContext _context;
        public VisitasController(AppDbContext context) => _context = context;

        // ========= REGISTRAR =========
        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar([FromBody] CrearVisitaDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return BadRequest("El nombre es obligatorio.");

            var visita = new VisitaDiaria
            {
                Nombre = dto.Nombre,
                Telefono = dto.Telefono ?? string.Empty,
                Correo = dto.Correo,
                FechaHoraIngreso = dto.FechaHoraIngreso ?? DateTime.Now,
                Costo = dto.Costo,
                FormaPago = string.IsNullOrWhiteSpace(dto.FormaPago) ? "Efectivo" : dto.FormaPago,
                Estado = EstadoVisita.Registrada
            };

            visita.Historial.Add(new VisitaHistorial
            {
                Accion = "Creado",
                Detalle = $"Ingreso: {visita.FechaHoraIngreso:yyyy-MM-dd HH:mm}"
            });

            _context.VisitasDiarias.Add(visita);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Visita registrada correctamente.", visita.Id });
        }

        // ========= ACTUALIZAR =========
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarVisitaDto dto)
        {
            var visita = await _context.VisitasDiarias.FindAsync(id);
            if (visita == null) return NotFound("Visita no encontrada.");
            if (visita.Estado == EstadoVisita.Cancelada)
                return BadRequest("No se puede editar una visita cancelada.");

            if (!string.IsNullOrWhiteSpace(dto.Nombre)) visita.Nombre = dto.Nombre;
            if (!string.IsNullOrWhiteSpace(dto.Telefono)) visita.Telefono = dto.Telefono!;
            if (!string.IsNullOrWhiteSpace(dto.Correo)) visita.Correo = dto.Correo;
            if (dto.FechaHoraIngreso.HasValue) visita.FechaHoraIngreso = dto.FechaHoraIngreso.Value;
            if (dto.Costo.HasValue) visita.Costo = dto.Costo.Value;
            if (!string.IsNullOrWhiteSpace(dto.FormaPago)) visita.FormaPago = dto.FormaPago!;

            _context.VisitasHistorial.Add(new VisitaHistorial
            {
                VisitaId = visita.Id,
                Accion = "Actualizado",
                Detalle = "Datos de la visita modificados"
            });

            await _context.SaveChangesAsync();
            return Ok(new { mensaje = "Visita actualizada." });
        }

        // ========= REGISTRAR SALIDA =========
        [HttpPut("salida/{id:int}")]
        public async Task<IActionResult> RegistrarSalida(int id)
        {
            var visita = await _context.VisitasDiarias.FindAsync(id);
            if (visita == null) return NotFound("Visita no encontrada.");
            if (visita.Estado == EstadoVisita.Cancelada)
                return BadRequest("No se puede registrar salida de una visita cancelada.");

            if (visita.FechaHoraSalida.HasValue)
                return BadRequest("La salida ya está registrada.");

            visita.FechaHoraSalida = DateTime.Now;
            visita.Estado = EstadoVisita.Completada;

            _context.VisitasHistorial.Add(new VisitaHistorial
            {
                VisitaId = visita.Id,
                Accion = "Salida",
                Detalle = $"Salida: {visita.FechaHoraSalida:yyyy-MM-dd HH:mm}"
            });

            await _context.SaveChangesAsync();
            return Ok(new { mensaje = "Salida registrada.", visita.Id });
        }

        // ========= CANCELAR =========
        [HttpPut("cancelar/{id:int}")]
        public async Task<IActionResult> Cancelar(int id)
        {
            var visita = await _context.VisitasDiarias.FindAsync(id);
            if (visita == null) return NotFound("Visita no encontrada.");
            if (visita.Estado == EstadoVisita.Cancelada)
                return BadRequest("La visita ya está cancelada.");

            visita.Estado = EstadoVisita.Cancelada;

            _context.VisitasHistorial.Add(new VisitaHistorial
            {
                VisitaId = visita.Id,
                Accion = "Cancelado",
                Detalle = "La visita fue cancelada por el administrador"
            });

            await _context.SaveChangesAsync();
            return Ok(new { mensaje = "Visita cancelada." });
        }

        // ========= LISTAR =========
        [HttpGet]
        public async Task<IActionResult> Listar([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta, [FromQuery] EstadoVisita? estado)
        {
            var q = _context.VisitasDiarias.AsQueryable();

            if (desde.HasValue) q = q.Where(v => v.FechaHoraIngreso >= desde.Value);
            if (hasta.HasValue) q = q.Where(v => v.FechaHoraIngreso < hasta.Value.AddDays(1));
            if (estado.HasValue) q = q.Where(v => v.Estado == estado.Value);

            var visitas = await q
                .OrderByDescending(v => v.FechaHoraIngreso)
                .Select(v => new
                {
                    v.Id,
                    v.Nombre,
                    v.Telefono,
                    v.Correo,
                    v.FechaHoraIngreso,
                    v.FechaHoraSalida,
                    v.Costo,
                    v.FormaPago,
                    Estado = v.Estado.ToString()
                })
                .ToListAsync();

            return Ok(visitas);
        }

        // ========= OBTENER POR ID =========
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Obtener(int id)
        {
            var visita = await _context.VisitasDiarias
                .Include(v => v.Historial)
                .Where(v => v.Id == id)
                .Select(v => new
                {
                    v.Id,
                    v.Nombre,
                    v.Telefono,
                    v.Correo,
                    v.FechaHoraIngreso,
                    v.FechaHoraSalida,
                    v.Costo,
                    v.FormaPago,
                    Estado = v.Estado.ToString(),
                    Historial = v.Historial
                        .OrderByDescending(h => h.Fecha)
                        .Select(h => new { h.Id, h.Accion, h.Fecha, h.Detalle })
                })
                .FirstOrDefaultAsync();

            if (visita == null) return NotFound("Visita no encontrada.");
            return Ok(visita);
        }

        // ========= HISTORIAL GLOBAL (opcional) =========
        [HttpGet("historial")]
        public async Task<IActionResult> HistorialGlobal([FromQuery] int? visitaId)
        {
            var q = _context.VisitasHistorial.AsQueryable();
            if (visitaId.HasValue) q = q.Where(h => h.VisitaId == visitaId.Value);

            var result = await q
                .OrderByDescending(h => h.Fecha)
                .Select(h => new { h.Id, h.VisitaId, h.Accion, h.Fecha, h.Detalle })
                .ToListAsync();

            return Ok(result);
        }

        // ========= MÉTRICAS para Dashboard =========
        [HttpGet("metricas")]
        public async Task<IActionResult> Metricas([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        {
            var hoy = DateTime.Today;
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

            // Rango opcional
            var q = _context.VisitasDiarias.AsQueryable();
            if (desde.HasValue) q = q.Where(v => v.FechaHoraIngreso >= desde.Value);
            if (hasta.HasValue) q = q.Where(v => v.FechaHoraIngreso < hasta.Value.AddDays(1));

            // Día
            var desdeDia = hoy;
            var hastaDia = hoy.AddDays(1);
            var qDia = q.Where(v => v.FechaHoraIngreso >= desdeDia && v.FechaHoraIngreso < hastaDia);

            // Mes
            var desdeMes = inicioMes;
            var hastaMes = inicioMes.AddMonths(1);
            var qMes = q.Where(v => v.FechaHoraIngreso >= desdeMes && v.FechaHoraIngreso < hastaMes);

            var visitasDia = await qDia.CountAsync();
            var ingresosDia = await qDia.Where(v => v.Estado != EstadoVisita.Cancelada).SumAsync(v => v.Costo);

            var visitasMes = await qMes.CountAsync();
            var ingresosMes = await qMes.Where(v => v.Estado != EstadoVisita.Cancelada).SumAsync(v => v.Costo);

            return Ok(new
            {
                dia = new { visitas = visitasDia, ingresos = ingresosDia },
                mes = new { visitas = visitasMes, ingresos = ingresosMes }
            });
        }
    }
}
