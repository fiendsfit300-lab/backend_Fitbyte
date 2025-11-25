namespace Gym_FitByte.DTOs
{
    public class EditarMembresiaDto
    {
        public string Nombre { get; set; } = string.Empty;
        public int Edad { get; set; }
        public string Telefono { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;

        public string Rutina { get; set; } = string.Empty;
        public string EnfermedadesOLesiones { get; set; } = string.Empty;

        // Foto opcional
        public IFormFile? Foto { get; set; }

        public string FormaPago { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Nivel { get; set; } = string.Empty;

        public decimal MontoPagado { get; set; }
        public DateTime FechaVencimiento { get; set; }
    }
}
