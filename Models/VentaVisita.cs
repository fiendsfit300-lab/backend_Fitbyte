using System.ComponentModel.DataAnnotations;

namespace Gym_FitByte.Models
{
    public class VentaVisita
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string NombreCliente { get; set; } = string.Empty;

        [Required]
        public decimal Costo { get; set; }

        [Required]
        public string FormaPago { get; set; } = "Efectivo";

        public DateTime FechaVenta { get; set; } = DateTime.Now;
    }
}
