using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_FitByte.Models
{
    public enum EstadoPreRegistro
    {
        Pendiente = 0,
        Aceptado = 1,
        Rechazado = 2,
        Vencido = 3
    }

    public class PreRegistro
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required, MaxLength(120)]
        public string Correo { get; set; } = string.Empty;

        [Required, MaxLength(15)]
        public string Telefono { get; set; } = string.Empty;

        public string Direccion { get; set; } = string.Empty;
        public int Edad { get; set; }

        [MaxLength(200)]
        public string? Observaciones { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        public EstadoPreRegistro Estado { get; set; } = EstadoPreRegistro.Pendiente;

        // ✅ Propiedad calculada, no mapeada en la BD
        [NotMapped]
        public DateTime FechaExpiracion => FechaRegistro.AddDays(3);
    }
}
