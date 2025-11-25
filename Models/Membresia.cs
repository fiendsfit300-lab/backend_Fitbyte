using System.ComponentModel.DataAnnotations;

namespace Gym_FitByte.Models
{
    public class Membresia
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(20)]
        public string CodigoCliente { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        public int Edad { get; set; }
        public string Telefono { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;

        [Required, MaxLength(120)]
        public string Correo { get; set; } = string.Empty;

        public string Rutina { get; set; } = string.Empty;
        public string EnfermedadesOLesiones { get; set; } = "Ninguna";
        public string FotoUrl { get; set; } = string.Empty;

        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        public DateTime FechaVencimiento { get; set; }

        public string FormaPago { get; set; } = "Efectivo";
        public string Tipo { get; set; } = "Inscripción";
        public string Nivel { get; set; } = "Básica";  

        public bool Activa { get; set; } = true;
        public decimal MontoPagado { get; set; }

        
        public ICollection<MembresiaHistorial>? Historial { get; set; }
    }
}
