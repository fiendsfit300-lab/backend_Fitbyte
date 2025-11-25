namespace Gym_FitByte.DTOs
{
    public class CrearVisitaDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string? Correo { get; set; }
        public DateTime? FechaHoraIngreso { get; set; }   // si viene null, se usa Now
        public decimal Costo { get; set; } = 0m;
        public string FormaPago { get; set; } = "Efectivo";
    }

    public class ActualizarVisitaDto
    {
        public string? Nombre { get; set; }
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
        public DateTime? FechaHoraIngreso { get; set; }
        public decimal? Costo { get; set; }
        public string? FormaPago { get; set; }
    }
}
