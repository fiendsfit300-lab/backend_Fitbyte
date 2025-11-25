using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_FitByte.Models
{
    public class Producto
    {
        [Key] public int Id { get; set; }

        [Required] public int ProveedorId { get; set; }
        public Proveedor? Proveedor { get; set; }

        [Required, MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        public decimal Precio { get; set; }

        [MaxLength(80)]
        public string Categoria { get; set; } = string.Empty;

        public string? FotoUrl { get; set; }

        public bool Activo { get; set; } = true;

        // ❗ Antes llamado Stock — esto NO es inventario
        // Es cuántas piezas trae cada paquete del proveedor
        public int PiezasPorPaquete { get; set; } = 1;
    }
}
