namespace Gym_FitByte.DTOs
{
    public class CrearVentaItemDto
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }

    public class CrearVentaDto
    {
        public string Cliente { get; set; } = "Mostrador";
        public DateTime FechaVenta { get; set; } = DateTime.Now;
        public string TipoVenta { get; set; } = "Mostrador";

        public List<CrearVentaItemDto> Items { get; set; } = new();
    }

    public class VentaDetalleDto
    {
        public int Id { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public DateTime FechaVenta { get; set; }
        public decimal Total { get; set; }
        public string TipoVenta { get; set; } = string.Empty;

        public List<VentaItemDetalleDto> Items { get; set; } = new();
    }

    public class VentaItemDetalleDto
    {
        public int ProductoId { get; set; }
        public string ProductoNombre { get; set; } = string.Empty;
        public string? FotoUrl { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}
