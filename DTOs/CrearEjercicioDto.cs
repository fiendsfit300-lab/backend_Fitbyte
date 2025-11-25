namespace Gym_FitByte.DTOs
{
    public class CrearEjercicioDto
    {
        public string Nombre { get; set; } = "";
        public string Series { get; set; } = "";
        public string Repeticiones { get; set; } = "";
        public string Descanso { get; set; } = "";
        public string Notas { get; set; } = "";
        public int RutinaId { get; set; }   // <- SELECCIONA UNA RUTINA
    }

}
