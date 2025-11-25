using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gym_FitByte.Data;
using Gym_FitByte.Models;
using Gym_FitByte.DTOs;

namespace Gym_FitByte.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public AdminsController(AppDbContext context) => _context = context;

        // Crear (Activo = true)
        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar([FromBody] AdminCrearDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Usuario) ||
                string.IsNullOrWhiteSpace(dto.Contrasena) ||
                string.IsNullOrWhiteSpace(dto.Nombre) ||
                string.IsNullOrWhiteSpace(dto.Correo))
                return BadRequest("Usuario, contraseña, nombre y correo son obligatorios.");

            var existeUsuario = await _context.Admins.AnyAsync(a => a.Usuario == dto.Usuario);
            if (existeUsuario) return BadRequest("El usuario ya existe.");

            var existeCorreo = await _context.Admins.AnyAsync(a => a.Correo == dto.Correo);
            if (existeCorreo) return BadRequest("El correo ya está en uso.");

            var admin = new Admin
            {
                Usuario = dto.Usuario.Trim(),
                Contrasena = dto.Contrasena, // TODO: Hashear en producción
                Nombre = dto.Nombre.Trim(),
                Correo = dto.Correo.Trim(),
                Telefono = dto.Telefono?.Trim() ?? string.Empty,
                Rol = string.IsNullOrWhiteSpace(dto.Rol) ? "Admin" : dto.Rol.Trim(),
                Activo = true,
                FechaRegistro = DateTime.Now
            };

            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Administrador registrado.",
                admin.Id,
                admin.Nombre,
                admin.Usuario,
                admin.Correo,
                admin.Telefono,
                admin.Rol,
                admin.Activo
            });
        }

        // Actualizar
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] AdminActualizarDto dto)
        {
            var admin = await _context.Admins.FindAsync(id);
            if (admin == null) return NotFound("Administrador no encontrado.");

            if (!string.IsNullOrWhiteSpace(dto.Usuario) && dto.Usuario != admin.Usuario)
            {
                var exists = await _context.Admins.AnyAsync(a => a.Usuario == dto.Usuario && a.Id != id);
                if (exists) return BadRequest("El nuevo usuario ya está en uso.");
                admin.Usuario = dto.Usuario.Trim();
            }

            if (!string.IsNullOrWhiteSpace(dto.Correo) && dto.Correo != admin.Correo)
            {
                var exists = await _context.Admins.AnyAsync(a => a.Correo == dto.Correo && a.Id != id);
                if (exists) return BadRequest("El nuevo correo ya está en uso.");
                admin.Correo = dto.Correo.Trim();
            }

            if (!string.IsNullOrWhiteSpace(dto.Contrasena))
                admin.Contrasena = dto.Contrasena; // TODO: Hashear

            if (!string.IsNullOrWhiteSpace(dto.Nombre))
                admin.Nombre = dto.Nombre.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Telefono))
                admin.Telefono = dto.Telefono.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Rol))
                admin.Rol = dto.Rol.Trim();

            if (dto.Activo.HasValue)
                admin.Activo = dto.Activo.Value;

            await _context.SaveChangesAsync();
            return Ok(new { mensaje = "Administrador actualizado." });
        }

        // Inactivar (soft delete)
        [HttpPut("inactivar/{id:int}")]
        public async Task<IActionResult> Inactivar(int id)
        {
            var admin = await _context.Admins.FindAsync(id);
            if (admin == null) return NotFound("Administrador no encontrado.");
            if (!admin.Activo) return BadRequest("El administrador ya está inactivo.");

            admin.Activo = false;
            await _context.SaveChangesAsync();
            return Ok(new { mensaje = "Administrador inactivado." });
        }

        // Activar
        [HttpPut("activar/{id:int}")]
        public async Task<IActionResult> Activar(int id)
        {
            var admin = await _context.Admins.FindAsync(id);
            if (admin == null) return NotFound("Administrador no encontrado.");
            if (admin.Activo) return BadRequest("El administrador ya está activo.");

            admin.Activo = true;
            await _context.SaveChangesAsync();
            return Ok(new { mensaje = "Administrador activado." });
        }

        // “Eliminar” = inactivar (por si quieres mantener DELETE en Swagger)
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var admin = await _context.Admins.FindAsync(id);
            if (admin == null) return NotFound("Administrador no encontrado.");

            admin.Activo = false;
            await _context.SaveChangesAsync();
            return Ok(new { mensaje = "Administrador marcado como inactivo." });
        }

        // Listar todos
        [HttpGet]
        public async Task<IActionResult> ListarTodos()
        {
            var lista = await _context.Admins
                .OrderByDescending(a => a.Id)
                .Select(a => new
                {
                    a.Id,
                    a.Nombre,
                    a.Usuario,
                    a.Correo,
                    a.Telefono,
                    a.Rol,
                    a.Activo,
                    a.FechaRegistro
                })
                .ToListAsync();

            return Ok(lista);
        }

        // Listar solo activos
        [HttpGet("activos")]
        public async Task<IActionResult> ListarActivos()
        {
            var lista = await _context.Admins
                .Where(a => a.Activo)
                .OrderBy(a => a.Nombre)
                .Select(a => new
                {
                    a.Id,
                    a.Nombre,
                    a.Usuario,
                    a.Correo,
                    a.Telefono,
                    a.Rol,
                    a.Activo
                })
                .ToListAsync();

            return Ok(lista);
        }

        // Obtener por Id
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Obtener(int id)
        {
            var admin = await _context.Admins
                .Where(a => a.Id == id)
                .Select(a => new
                {
                    a.Id,
                    a.Nombre,
                    a.Usuario,
                    a.Correo,
                    a.Telefono,
                    a.Rol,
                    a.Activo,
                    a.FechaRegistro
                })
                .FirstOrDefaultAsync();

            if (admin == null) return NotFound("Administrador no encontrado.");
            return Ok(admin);
        }
    }
}
