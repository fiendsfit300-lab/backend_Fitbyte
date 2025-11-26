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

        // 🔹 Precio del PAQUETE (costo proveedor)
        [Column(TypeName = "decimal(10,2)")]
        public decimal Precio { get; set; }

        // 🔹 Costo por pieza (Precio / PiezasPorPaquete) — se calcula
        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioUnitario { get; set; }

        // 🔥 Precio de VENTA final por pieza (con ganancia) — lo defines tú
        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioFinal { get; set; }

        [MaxLength(80)]
        public string Categoria { get; set; } = string.Empty;

        public string? FotoUrl { get; set; }

        public bool Activo { get; set; } = true;

        // Cuántas piezas trae cada paquete del proveedor
        public int PiezasPorPaquete { get; set; } = 1;
    }
}
