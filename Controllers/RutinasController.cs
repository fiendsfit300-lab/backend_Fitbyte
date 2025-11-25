using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Gym_FitByte.Data;
using Gym_FitByte.DTOs;
using Gym_FitByte.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gym_FitByte.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RutinasController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private const string ContainerName = "rutinas";

        public RutinasController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // ============================
        // CREATE RUTINA
        // ============================
        [HttpPost("crear")]
        public async Task<IActionResult> CrearRutina([FromForm] CrearRutinaDto dto)
        {
            string imagenUrl = "";

            if (dto.Imagen != null)
                imagenUrl = await SubirImagen(dto.Imagen);

            var rutina = new Rutina
            {
                Titulo = dto.Titulo,
                Descripcion = dto.Descripcion,
                Duracion = dto.Duracion,
                Genero = dto.Genero,
                Nivel = dto.Nivel,
                ImagenUrl = imagenUrl
            };

            _context.Rutinas.Add(rutina);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Rutina creada correctamente",
                rutina
            });
        }

        // ============================
        // UPDATE RUTINA
        // ============================
        [HttpPut("actualizar/{id:int}")]
        public async Task<IActionResult> ActualizarRutina(int id, [FromForm] CrearRutinaDto dto)
        {
            var rutina = await _context.Rutinas.FindAsync(id);

            if (rutina == null)
                return NotFound("La rutina no existe.");

            // Si viene imagen nueva → reemplazarla
            if (dto.Imagen != null)
            {
                rutina.ImagenUrl = await SubirImagen(dto.Imagen);
            }

            rutina.Titulo = dto.Titulo;
            rutina.Descripcion = dto.Descripcion;
            rutina.Duracion = dto.Duracion;
            rutina.Genero = dto.Genero;
            rutina.Nivel = dto.Nivel;

            await _context.SaveChangesAsync();

            return Ok("Rutina actualizada correctamente.");
        }

        // ============================
        // LISTAR TODAS
        // ============================
        [HttpGet]
        public async Task<IActionResult> ObtenerTodas()
        {
            var data = await _context.Rutinas
                .OrderByDescending(r => r.Id)
                .ToListAsync();

            return Ok(data);
        }

        // ============================
        // DETALLE
        // ============================
        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerRutina(int id)
        {
            var rutina = await _context.Rutinas
                .Include(r => r.Ejercicios)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rutina == null)
                return NotFound("No existe la rutina.");

            return Ok(rutina);
        }

        // ============================
        // DELETE
        // ============================
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> EliminarRutina(int id)
        {
            var rutina = await _context.Rutinas.FindAsync(id);
            if (rutina == null)
                return NotFound();

            _context.Rutinas.Remove(rutina);
            await _context.SaveChangesAsync();

            return Ok("Rutina eliminada.");
        }

        // ============================
        // SUBIR IMAGEN
        // ============================
        private async Task<string> SubirImagen(IFormFile archivo)
        {
            var connectionString = _config.GetConnectionString("AzureBlobStorage");

            var service = new BlobServiceClient(connectionString);
            var container = service.GetBlobContainerClient(ContainerName);

            await container.CreateIfNotExistsAsync();
            await container.SetAccessPolicyAsync(PublicAccessType.Blob);

            string nombre = $"{Guid.NewGuid()}{Path.GetExtension(archivo.FileName)}";

            var blob = container.GetBlobClient(nombre);

            using var stream = archivo.OpenReadStream();
            await blob.UploadAsync(stream, overwrite: true);

            return blob.Uri.ToString();
        }
    }
}
