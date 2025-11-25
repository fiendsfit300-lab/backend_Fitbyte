using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Gym_FitByte.Data;
using Gym_FitByte.Models;

namespace Gym_FitByte.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProgresosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private const string ContainerName = "fotos"; 

        public ProgresosController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

   
        [HttpPost("registrar")]
        public async Task<IActionResult> RegistrarProgreso([FromForm] CrearProgresoDto dto)
        {
            if (string.IsNullOrEmpty(dto.CodigoCliente))
                return BadRequest("El código de cliente es obligatorio.");

            string? urlFoto = null;

        
            if (dto.Foto != null && dto.Foto.Length > 0)
            {
                urlFoto = await SubirFotoABlob(dto.Foto);
            }

            var nuevo = new Progreso
            {
                CodigoCliente = dto.CodigoCliente,
                Peso = dto.Peso,
                Pecho = dto.Pecho,
                Cintura = dto.Cintura,
                Brazo = dto.Brazo,
                Pierna = dto.Pierna,
                Hombros = dto.Hombros,
                FotoUrl = urlFoto,
                FechaRegistro = DateTime.Now
            };

            _context.Progresos.Add(nuevo);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Progreso guardado correctamente.",
                nuevo.Id,
                nuevo.CodigoCliente,
                nuevo.FotoUrl,
                nuevo.FechaRegistro
            });
        }

    
        [HttpGet("codigo/{codigo}")]
        public async Task<IActionResult> ObtenerPorCliente(string codigo)
        {
            var lista = await _context.Progresos
                .Where(p => p.CodigoCliente == codigo)
                .OrderByDescending(p => p.FechaRegistro)
                .ToListAsync();

            if (!lista.Any())
                return NotFound(new { mensaje = "No se encontraron registros de progreso." });

            return Ok(lista);
        }
 
        [HttpGet("ultimo/{codigo}")]
        public async Task<IActionResult> ObtenerUltimo(string codigo)
        {
            var ultimo = await _context.Progresos
                .Where(p => p.CodigoCliente == codigo)
                .OrderByDescending(p => p.FechaRegistro)
                .FirstOrDefaultAsync();

            if (ultimo == null)
                return NotFound(new { mensaje = "No se encontró un registro de progreso." });

            return Ok(ultimo);
        }

 
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarProgreso(int id)
        {
            var progreso = await _context.Progresos.FindAsync(id);
            if (progreso == null)
                return NotFound(new { mensaje = "Registro no encontrado." });

            _context.Progresos.Remove(progreso);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Progreso eliminado correctamente." });
        }
 
        private async Task<string> SubirFotoABlob(IFormFile archivo)
        {
            var connectionString = _config.GetConnectionString("AzureBlobStorage");
            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(ContainerName);

            await containerClient.CreateIfNotExistsAsync();
            await containerClient.SetAccessPolicyAsync(PublicAccessType.Blob);

            var nombreArchivo = $"{Guid.NewGuid()}{Path.GetExtension(archivo.FileName)}";
            var blobClient = containerClient.GetBlobClient(nombreArchivo);

            using var stream = archivo.OpenReadStream();
            await blobClient.UploadAsync(stream, overwrite: true);

            return blobClient.Uri.ToString();
        }

  
        public class CrearProgresoDto
        {
            public string CodigoCliente { get; set; } = string.Empty;
            public double Peso { get; set; }
            public double Pecho { get; set; }
            public double Cintura { get; set; }
            public double Brazo { get; set; }
            public double Pierna { get; set; }
            public double Hombros { get; set; }
            public IFormFile? Foto { get; set; }  
        }
    }
}
