using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_FitByte.Models
{
    public enum EstadoVisita
    {
        Registrada = 0,   // Ingresó o está por ingresar
        Completada = 1,   // Se registró la salida
        Cancelada = 2    // Se anuló la visita
    }

    public class VisitaDiaria
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(120)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Telefono { get; set; } = string.Empty;

        [MaxLength(150), EmailAddress]
        public string? Correo { get; set; }

        // Fechas / horas
        public DateTime FechaHoraIngreso { get; set; } = DateTime.Now;
        public DateTime? FechaHoraSalida { get; set; }

        // Pago (opcional pero útil para Dashboard)
        [Column(TypeName = "decimal(10,2)")]
        public decimal Costo { get; set; } = 0m;

        [MaxLength(40)]
        public string FormaPago { get; set; } = "Efectivo";

        public EstadoVisita Estado { get; set; } = EstadoVisita.Registrada;

        // Navegación
        public ICollection<VisitaHistorial> Historial { get; set; } = new List<VisitaHistorial>();
    }
}
