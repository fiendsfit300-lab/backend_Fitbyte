using Gym_FitByte.Models;

namespace Gym_FitByte.DTOs
{
    public class CompraDetalleDto
    {
        public int Id { get; set; }
        public DateTime FechaCompra { get; set; }
        public decimal Total { get; set; }
        public string? Folio { get; set; }
        public string? Comentarios { get; set; }

        public List<CompraItemDetalleDto> Items { get; set; } = new();
    }

    public class CompraItemDetalleDto
    {
        public int ProductoId { get; set; }
        public string ProductoNombre { get; set; } = string.Empty;
        public string? FotoUrl { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public int ProveedorId { get; set; }
        public string ProveedorNombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class CrearCompraItemDto
    {
        public int ProductoId { get; set; }
        public int ProveedorId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }

        public string? Nombre { get; set; }
        public string? Categoria { get; set; }
        public string? FotoUrl { get; set; }
    }

    public class CrearCompraDto
    {
        public DateTime FechaCompra { get; set; } = DateTime.Now;
        public string? Folio { get; set; }
        public string? Comentarios { get; set; }

        public List<CrearCompraItemDto> Items { get; set; } = new();
    }
}
