using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_FitByte.Models
{
    public enum TipoMovimiento { Entrada = 1, Salida = 2, Ajuste = 3 }

    public class HistorialMovimiento
    {
        [Key] public int Id { get; set; }

        [Required]
        public int ProductoId { get; set; }
        public Producto? Producto { get; set; }

        [Required]
        public TipoMovimiento Tipo { get; set; }

        [Required]
        public int Cantidad { get; set; }

        public string? Motivo { get; set; }

        public int? CompraId { get; set; }
        public int? VentaId { get; set; }

        public DateTime FechaMovimiento { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(10,2)")]
        public decimal? PrecioUnitario { get; set; }

        [MaxLength(100)]
        public string? Referencia { get; set; }
    }
}
