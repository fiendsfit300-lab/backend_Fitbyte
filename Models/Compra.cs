using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_FitByte.Models
{
    public class Compra
    {
        [Key] public int Id { get; set; }

        public DateTime FechaCompra { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(10,2)")]
        public decimal Total { get; set; }

        [MaxLength(50)]
        public string? Folio { get; set; }

        public string? Comentarios { get; set; }

        public ICollection<CompraItem> Items { get; set; } = new List<CompraItem>();
    }

    public class CompraItem
    {
        [Key] public int Id { get; set; }

        [Required] public int CompraId { get; set; }
        public Compra? Compra { get; set; }

        [Required] public int ProductoId { get; set; }
        public Producto? Producto { get; set; }

        /// <summary>
        /// Cantidad de paquetes comprados
        /// </summary>
        [Required]
        public int Cantidad { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioUnitario { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Subtotal { get; set; }

        /// <summary>
        /// Indica si ya se reflejó en inventario
        /// </summary>
        public bool InventarioActualizado { get; set; } = false;
    }
}
