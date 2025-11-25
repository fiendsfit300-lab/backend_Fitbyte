using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gym_FitByte.Data;
using Gym_FitByte.Models;

namespace Gym_FitByte.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthClienteController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthClienteController(AppDbContext context)
        {
            _context = context;
        }
 
        [HttpPost("login")]
        public async Task<IActionResult> LoginCliente([FromBody] LoginCliente dto)
        {
            if (string.IsNullOrEmpty(dto.CodigoCliente) || string.IsNullOrEmpty(dto.Nombre))
                return BadRequest(new { mensaje = "Debe ingresar su código y nombre." });

           
            var cliente = await _context.Membresias
                .FirstOrDefaultAsync(m => m.CodigoCliente == dto.CodigoCliente && m.Nombre == dto.Nombre);

            if (cliente == null)
                return NotFound(new { mensaje = "No se encontró un cliente con esos datos." });

           
            if (!cliente.Activa || cliente.FechaVencimiento < DateTime.Now)
            {
                cliente.Activa = false;
                _context.Membresias.Update(cliente);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    acceso = false,
                    mensaje = "Tu membresía está inactiva o vencida. Por favor realiza tu pago para continuar.",
                    cliente.Nombre,
                    cliente.FechaVencimiento
                });
            }

            
            return Ok(new
            {
                acceso = true,
                mensaje = "Inicio de sesión exitoso. ¡Bienvenido/a " + cliente.Nombre + "!",
                cliente.Id,
                cliente.Nombre,
                cliente.CodigoCliente,
                cliente.Nivel,
                cliente.FotoUrl,
                cliente.FechaVencimiento
            });
        }
    }
}
