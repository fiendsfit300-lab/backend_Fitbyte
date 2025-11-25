namespace Gym_FitByte.Models
{
    public class Ejercicio
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Series { get; set; } = "";
        public string Repeticiones { get; set; } = "";
        public string Descanso { get; set; } = "";
        public string Notas { get; set; } = "";

        public int RutinaId { get; set; }
        public Rutina? Rutina { get; set; }
    }
}
