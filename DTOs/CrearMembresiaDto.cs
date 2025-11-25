namespace Gym_FitByte.DTOs
{
    public class CrearMembresiaDto
    {
        public string Nombre { get; set; }
        public int Edad { get; set; }
        public string Telefono { get; set; }
        public string Direccion { get; set; }
        public string Correo { get; set; }
        public string Rutina { get; set; }
        public string EnfermedadesOLesiones { get; set; }
        //public IFormFile Foto { get; set; }   // 🔥 OBLIGATORIO
        public IFormFile? Foto { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string FormaPago { get; set; }
        public string Tipo { get; set; }
        public decimal MontoPagado { get; set; }
        public string Nivel { get; set; }
    }
}
