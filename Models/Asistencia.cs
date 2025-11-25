using System.ComponentModel.DataAnnotations;

namespace Gym_FitByte.Models
{
    public class Asistencia
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string CodigoCliente { get; set; } = string.Empty;

        public DateTime FechaHora { get; set; } = DateTime.Now;
    }
}