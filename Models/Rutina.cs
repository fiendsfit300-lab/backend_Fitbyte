namespace Gym_FitByte.Models
{
    public class Rutina
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public string Duracion { get; set; } = "";
        public string Nivel { get; set; } = "";
        public string Genero { get; set; } = "";
        public string ImagenUrl { get; set; } = "";

        public List<Ejercicio> Ejercicios { get; set; } = new();
    }
}
