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

        // ============================================================
        // 🔥 FUNCIÓN: REGISTRAR MOVIMIENTO EN CORTE DE CAJA
        // ============================================================
        private async Task RegistrarMovimiento(string tipo, decimal monto, string descripcion)
        {
            var corte = await _context.CortesCaja.FirstOrDefaultAsync(c => c.Estado == 0);

            if (corte == null)
                return; // No hay corte abierto

            var mov = new MovimientoCaja
            {
                CorteCajaId = corte.Id,
                Tipo = tipo,
                Monto = monto,
                Descripcion = descripcion,
                Fecha = DateTime.Now
            };

            _context.MovimientosCaja.Add(mov);
            await _context.SaveChangesAsync();
        }

        // ============================================================
        // 🔥 REGISTRAR NUEVA MEMBRESÍA
        // ============================================================
        [HttpPost("registrar")]
        public async Task<IActionResult> RegistrarMembresia([FromForm] CrearMembresiaDto dto)
        {
            // Cargar foto (si existe)
            string urlFoto;

            if (dto.Foto != null && dto.Foto.Length > 0)
            {
                urlFoto = await SubirFotoABlob(dto.Foto);
            }
            else
            {
                urlFoto = "https://placehold.co/300x300?text=Sin+Foto";
            }

            // Generar código único
            string codigo = await GenerarCodigoUnicoAsync();

            // Crear membresía
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

            // 🔥 REGISTRAR MOVIMIENTO EN CORTE (INSCRIPCIÓN)
            await RegistrarMovimiento("Membresía", m.MontoPagado,
                $"Nueva membresía #{m.Id} — {m.Nombre}");

            return Ok(new
            {
                mensaje = "Membresía registrada correctamente.",
                m.CodigoCliente,
                m.Nombre,
                m.Nivel
            });
        }

        // ============================================================
        // 🔥 RENOVAR MEMBRESÍA
        // ============================================================
        [HttpPut("renovar/{id:int}")]
        public async Task<IActionResult> RenovarMembresia(int id, [FromBody] RenovarMembresiaDto dto)
        {
            var m = await _context.Membresias.FirstOrDefaultAsync(x => x.Id == id);
            if (m == null)
                return NotFound("Membresía no encontrada.");

            if (dto.NuevaFechaVencimiento <= DateTime.UtcNow.Date)
                return BadRequest("La nueva fecha debe ser mayor a hoy.");

            // Registrar historial de renovación
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

            // Actualizar datos
            m.FechaVencimiento = dto.NuevaFechaVencimiento;
            m.FormaPago = dto.TipoPago;
            m.MontoPagado = dto.MontoPagado;
            m.Activa = true;

            _context.Membresias.Update(m);
            await _context.SaveChangesAsync();

            // 🔥 REGISTRAR MOVIMIENTO DE CORTE (RENOVACIÓN)
            await RegistrarMovimiento("Renovación", dto.MontoPagado,
                $"Renovación #{m.Id} — {m.Nombre}");

            return Ok(new
            {
                mensaje = "Membresía renovada correctamente.",
                m.CodigoCliente,
                m.FechaVencimiento,
                m.FormaPago,
                m.MontoPagado
            });
        }

        // ============================================================
        // 🔧 EDITAR MEMBRESÍA
        // ============================================================
        [HttpPut("editar/{id:int}")]
        public async Task<IActionResult> EditarMembresia(int id, [FromForm] EditarMembresiaDto dto)
        {
            var m = await _context.Membresias.FirstOrDefaultAsync(x => x.Id == id);
            if (m == null)
                return NotFound("Membresía no encontrada.");

            if (dto.Foto != null && dto.Foto.Length > 0)
                m.FotoUrl = await SubirFotoABlob(dto.Foto);

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

        // ============================================================
        // OBTENER POR VENCER
        // ============================================================
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

        // ============================================================
        // OBTENER POR CÓDIGO
        // ============================================================
        [HttpGet("codigo/{codigo}")]
        public async Task<IActionResult> ObtenerPorCodigo(string codigo)
        {
            var m = await _context.Membresias
                .FirstOrDefaultAsync(x => x.CodigoCliente == codigo);

            if (m == null)
                return NotFound("No se encontró una membresía con ese código.");

            return Ok(m);
        }

        // ============================================================
        // HISTORIAL DE PAGOS
        // ============================================================
        [HttpGet("historial/{codigo}")]
        public async Task<IActionResult> ObtenerHistorial(string codigo)
        {
            var data = await _context.MembresiasHistorial
                .Where(h => h.CodigoCliente == codigo)
                .OrderByDescending(h => h.FechaPago)
                .ToListAsync();

            return Ok(data);
        }

        // ============================================================
        // OBTENER TODAS
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> ObtenerTodas()
        {
            var membresias = await _context.Membresias
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            return Ok(membresias);
        }

        // ============================================================
        // 🔧 SUBIR FOTO AL BLOB
        // ============================================================
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

        // ============================================================
        // GENERAR CÓDIGO ÚNICO
        // ============================================================
        private async Task<string> GenerarCodigoUnicoAsync()
        {
            string codigo;
            Random random = new();

            do
            {
                codigo = random.Next(100000, 999999).ToString();
            }
            while (await _context.Membresias.AnyAsync(m => m.CodigoCliente == codigo));

            return codigo;
        }
    }
}
