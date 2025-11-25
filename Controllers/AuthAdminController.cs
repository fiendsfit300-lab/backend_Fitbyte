using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gym_FitByte.Data;
using Gym_FitByte.Models;
using Gym_FitByte.DTOs;

namespace Gym_FitByte.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthAdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthAdminController(AppDbContext context)
        {
            _context = context;
        }

        // ======================
        //     LOGIN ADMIN
        // ======================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AdminLoginDto dto)
        {
            if (string.IsNullOrEmpty(dto.Usuario) || string.IsNullOrEmpty(dto.Contrasena))
                return BadRequest("Usuario y contraseña son requeridos.");

            var admin = await _context.Admins
                .FirstOrDefaultAsync(a => a.Usuario == dto.Usuario);

            if (admin == null)
                return Unauthorized("Usuario no encontrado.");

            if (!admin.Activo)
                return Unauthorized("Usuario inactivo. Contacte al administrador.");

            if (admin.Contrasena != dto.Contrasena)
                return Unauthorized("Contraseña incorrecta.");

            return Ok(new
            {
                message = "Inicio de sesión exitoso",
                admin.Id,
                admin.Nombre,
                admin.Usuario,
                admin.Correo,
                admin.Telefono,
                admin.Rol
            });
        }
    }

    // DTO para el login
    public class AdminLoginDto
    {
        public string Usuario { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
    }
}
