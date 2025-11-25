using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gym_FitByte.Data;
using Gym_FitByte.Models;
using Gym_FitByte.DTOs;

namespace Gym_FitByte.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProveedoresController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProveedoresController(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // REGISTRAR PROVEEDOR
        // ============================================================
        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar([FromBody] CrearProveedorDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NombreEmpresa))
                return BadRequest("El nombre de empresa es obligatorio.");

            if (string.IsNullOrWhiteSpace(dto.RFC))
                return BadRequest("El RFC es obligatorio.");

            bool existeRFC = await _context.Proveedores
                .AnyAsync(p => p.RFC == dto.RFC);

            if (existeRFC)
                return Conflict("Ya existe un proveedor con ese RFC.");

            var proveedor = new Proveedor
            {
                NombreEmpresa = dto.NombreEmpresa,
                PersonaContacto = dto.PersonaContacto,
                Telefono = dto.Telefono,
                Email = dto.Email,
                Direccion = dto.Direccion,
                RFC = dto.RFC,
                Activo = true
            };

            _context.Proveedores.Add(proveedor);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Proveedor registrado correctamente.",
                proveedor.Id
            });
        }

        // ============================================================
        // ACTUALIZAR PROVEEDOR
        // ============================================================
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarProveedorDto dto)
        {
            var proveedor = await _context.Proveedores.FindAsync(id);
            if (proveedor == null)
                return NotFound("Proveedor no encontrado.");

            // Validar RFC duplicado si lo cambiaron
            if (!string.Equals(proveedor.RFC, dto.RFC, StringComparison.OrdinalIgnoreCase))
            {
                bool existeRFC = await _context.Proveedores
                    .AnyAsync(p => p.RFC == dto.RFC);

                if (existeRFC)
                    return Conflict("Ya existe un proveedor con ese RFC.");
            }

            proveedor.NombreEmpresa = dto.NombreEmpresa;
            proveedor.PersonaContacto = dto.PersonaContacto;
            proveedor.Telefono = dto.Telefono;
            proveedor.Email = dto.Email;
            proveedor.Direccion = dto.Direccion;
            proveedor.RFC = dto.RFC;
            proveedor.Activo = dto.Activo;

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Proveedor actualizado correctamente." });
        }

        // ============================================================
        // DESACTIVAR PROVEEDOR
        // ============================================================
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Desactivar(int id)
        {
            var proveedor = await _context.Proveedores.FindAsync(id);
            if (proveedor == null)
                return NotFound("Proveedor no encontrado.");

            proveedor.Activo = false;
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Proveedor desactivado correctamente." });
        }

        // ============================================================
        // ACTIVAR PROVEEDOR
        // ============================================================
        [HttpPut("activar/{id:int}")]
        public async Task<IActionResult> Activar(int id)
        {
            var proveedor = await _context.Proveedores.FindAsync(id);
            if (proveedor == null)
                return NotFound("Proveedor no encontrado.");

            proveedor.Activo = true;
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Proveedor activado correctamente." });
        }

        // ============================================================
        // LISTAR PROVEEDORES ACTIVOS
        // ============================================================
        [HttpGet("activos")]
        public async Task<IActionResult> Activos()
        {
            var lista = await _context.Proveedores
                .Where(p => p.Activo)
                .OrderByDescending(p => p.Id)
                .Select(p => new
                {
                    p.Id,
                    p.NombreEmpresa,
                    p.PersonaContacto,
                    p.Telefono,
                    p.Email,
                    p.Direccion,
                    p.RFC,
                    p.Activo
                })
                .ToListAsync();

            return Ok(lista);
        }

        // ============================================================
        // DETALLE DE PROVEEDOR + SUS PRODUCTOS ACTIVOS
        // ============================================================
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Detalle(int id)
        {
            var proveedor = await _context.Proveedores
                .Include(p => p.Productos!.Where(x => x.Activo))
                .FirstOrDefaultAsync(p => p.Id == id);

            if (proveedor == null)
                return NotFound("Proveedor no encontrado.");

            return Ok(new
            {
                proveedor.Id,
                proveedor.NombreEmpresa,
                proveedor.PersonaContacto,
                proveedor.Telefono,
                proveedor.Email,
                proveedor.Direccion,
                proveedor.RFC,
                proveedor.Activo,
                Productos = proveedor.Productos?.Select(pr => new
                {
                    pr.Id,
                    pr.Nombre,
                    pr.Precio,
                    pr.Categoria,
                    pr.FotoUrl,
                    pr.PiezasPorPaquete,
                    pr.Activo
                })
            });
        }
    }
}
