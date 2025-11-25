namespace Gym_FitByte.DTOs
{
    public class AjusteInventarioDto
    {
        public int ProductoId { get; set; }
        public int NuevaCantidad { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public string? Referencia { get; set; }
    }
}
