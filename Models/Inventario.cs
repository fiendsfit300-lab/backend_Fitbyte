using System.ComponentModel.DataAnnotations;

namespace Gym_FitByte.Models
{
    public class Inventario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductoId { get; set; }
        public Producto? Producto { get; set; }

        [Required]
        public int Cantidad { get; set; }

        public DateTime FechaActualizacion { get; set; } = DateTime.Now;

        public int CantidadComprada { get; set; }
        public int CantidadVendida { get; set; }
    }
}
