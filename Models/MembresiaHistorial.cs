using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_FitByte.Models
{
    public class MembresiaHistorial
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(20)]
        public string CodigoCliente { get; set; } = string.Empty;

        public DateTime FechaPago { get; set; } = DateTime.UtcNow;
        public DateTime PeriodoInicio { get; set; }
        public DateTime PeriodoFin { get; set; }

        [MaxLength(40)]
        public string FormaPago { get; set; } = "Efectivo";

        [Column(TypeName = "decimal(10,2)")]
        public decimal MontoPagado { get; set; }

        public int? MembresiaId { get; set; }
        public Membresia? Membresia { get; set; }
    }
}
