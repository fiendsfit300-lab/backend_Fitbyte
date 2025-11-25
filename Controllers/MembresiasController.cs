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
    public class MembresiasController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private const string ContainerName = "fotos";

        public MembresiasController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }
        [HttpPost("registrar")]
        public async Task<IActionResult> RegistrarMembresia([FromForm] CrearMembresiaDto dto)
        {
            // FOTO OPCIONAL — si no envían foto NO generará error
            string urlFoto;

            if (dto.Foto != null && dto.Foto.Length > 0)
            {
                urlFoto = await SubirFotoABlob(dto.Foto);
            }
            else
            {
                urlFoto = "https://placehold.co/300x300?text=Sin+Foto";
            }

            string codigo = await GenerarCodigoUnicoAsync();

            var m = new Membresia
            {
                CodigoCliente = codigo,
                Nombre = dto.Nombre,
                Edad = dto.Edad,
                Telefono = dto.Telefono,
                Direccion = dto.Direccion,
                Correo = dto.Correo,
                Rutina = dto.Rutina,
                EnfermedadesOLesiones = dto.EnfermedadesOLesiones,
                FotoUrl = urlFoto,
                FechaRegistro = dto.FechaRegistro,
                FechaVencimiento = dto.FechaVencimiento,
                FormaPago = dto.FormaPago,
                Tipo = dto.Tipo,
                MontoPagado = dto.MontoPagado,
                Activa = true,
                Nivel = dto.Nivel
            };

            _context.Membresias.Add(m);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Membresía registrada correctamente.",
                m.CodigoCliente,
                m.Nombre,
                m.Nivel
            });
        }



        [HttpPut("renovar/{id:int}")]
        public async Task<IActionResult> RenovarMembresia(int id, [FromBody] RenovarMembresiaDto dto)
        {
            var m = await _context.Membresias.FirstOrDefaultAsync(x => x.Id == id);
            if (m == null) return NotFound("Membresía no encontrada.");

            if (dto.NuevaFechaVencimiento <= DateTime.UtcNow.Date)
                return BadRequest("La nueva fecha de vencimiento debe ser posterior a hoy.");

         
            var historial = new MembresiaHistorial
            {
                MembresiaId = m.Id,
                CodigoCliente = m.CodigoCliente,
                FechaPago = DateTime.UtcNow,
                PeriodoInicio = m.FechaVencimiento.AddDays(1),
                PeriodoFin = dto.NuevaFechaVencimiento,
                FormaPago = dto.TipoPago,
                MontoPagado = dto.MontoPagado
            };

            _context.MembresiasHistorial.Add(historial);

          
            m.FechaVencimiento = dto.NuevaFechaVencimiento;
            m.FormaPago = dto.TipoPago;
            m.MontoPagado = dto.MontoPagado;
            m.Activa = true;

            _context.Membresias.Update(m);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Membresía renovada correctamente.",
                m.CodigoCliente,
                m.FechaVencimiento,
                m.FormaPago,
                m.MontoPagado
            });
        }

        [HttpPut("editar/{id:int}")]
        public async Task<IActionResult> EditarMembresia(int id, [FromForm] EditarMembresiaDto dto)
        {
            var m = await _context.Membresias.FirstOrDefaultAsync(x => x.Id == id);
            if (m == null)
                return NotFound("Membresía no encontrada.");

            // SI SE ENVÍA FOTO → SUBIR Y REEMPLAZAR
            if (dto.Foto != null && dto.Foto.Length > 0)
            {
                m.FotoUrl = await SubirFotoABlob(dto.Foto);
            }

            // ACTUALIZAR CAMPOS
            m.Nombre = dto.Nombre;
            m.Edad = dto.Edad;
            m.Telefono = dto.Telefono;
            m.Direccion = dto.Direccion;
            m.Correo = dto.Correo;

            m.Rutina = dto.Rutina;
            m.EnfermedadesOLesiones = dto.EnfermedadesOLesiones;

            m.FormaPago = dto.FormaPago;
            m.Tipo = dto.Tipo;
            m.Nivel = dto.Nivel;
            m.MontoPagado = dto.MontoPagado;

            m.FechaVencimiento = dto.FechaVencimiento;

            _context.Membresias.Update(m);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Membresía actualizada correctamente.",
                m.Id,
                m.Nombre,
                m.CodigoCliente
            });
        }


        [HttpGet("por-vencer")]
        public async Task<IActionResult> ObtenerPorVencer()
        {
            var hoy = DateTime.Now.Date;
            var limite = hoy.AddDays(2);

            var porVencer = await _context.Membresias
                .Where(m => m.FechaVencimiento.Date >= hoy
                            && m.FechaVencimiento.Date <= limite
                            && m.Activa)
                .ToListAsync();

            return Ok(porVencer);
        }




        [HttpGet("codigo/{codigo}")]
        public async Task<IActionResult> ObtenerPorCodigo(string codigo)
        {
            var m = await _context.Membresias
                .FirstOrDefaultAsync(x => x.CodigoCliente == codigo);

            if (m == null)
                return NotFound("No se encontró una membresía con ese código.");

            return Ok(m);
        }

        
        [HttpGet("historial/{codigo}")]
        public async Task<IActionResult> ObtenerHistorial(string codigo)
        {
            var data = await _context.MembresiasHistorial
                .Where(h => h.CodigoCliente == codigo)
                .OrderByDescending(h => h.FechaPago)
                .ToListAsync();

            return Ok(data);
        }

        
        [HttpGet]
        public async Task<IActionResult> ObtenerTodas()
        {
            var membresias = await _context.Membresias
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            return Ok(membresias);
        }

     

        private async Task<string> SubirFotoABlob(IFormFile archivo)
        {
            var connectionString = _config.GetConnectionString("AzureBlobStorage");
            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(ContainerName);

            await containerClient.CreateIfNotExistsAsync();
            await containerClient.SetAccessPolicyAsync(PublicAccessType.Blob);

            var nombre = $"{Guid.NewGuid()}{Path.GetExtension(archivo.FileName)}";
            var blobClient = containerClient.GetBlobClient(nombre);

            using var stream = archivo.OpenReadStream();
            await blobClient.UploadAsync(stream, overwrite: true);

            return blobClient.Uri.ToString();
        }

        private async Task<string> GenerarCodigoUnicoAsync()
        {
            string codigo;
            Random random = new();

            do
            {
                codigo = random.Next(100000, 999999).ToString();
            } while (await _context.Membresias.AnyAsync(m => m.CodigoCliente == codigo));

            return codigo;
        }
    }

   
    public class CrearMembresiaDto
    {
        public string Nombre { get; set; } = string.Empty;
        public int Edad { get; set; }
        public string Telefono { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Rutina { get; set; } = string.Empty;
        public string EnfermedadesOLesiones { get; set; } = "Ninguna";
        public IFormFile Foto { get; set; } = default!;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        public DateTime FechaVencimiento { get; set; }
        public string FormaPago { get; set; } = "Efectivo";
        public string Tipo { get; set; } = "Inscripción";
        public decimal MontoPagado { get; set; }
        public string Nivel { get; set; } = "Básica";
    }

    public class RenovarMembresiaDto
    {
        public DateTime NuevaFechaVencimiento { get; set; }
        public string TipoPago { get; set; } = "Efectivo";
        public decimal MontoPagado { get; set; }
    }
}
