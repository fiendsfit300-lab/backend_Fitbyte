using System.ComponentModel.DataAnnotations;

namespace Gym_FitByte.Models
{
    public class Admin
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(60)]
        public string Usuario { get; set; } = string.Empty;   // único

        [Required, MaxLength(120)]
        public string Contrasena { get; set; } = string.Empty; // *recomendado* hashear

        [Required, MaxLength(120)]
        public string Nombre { get; set; } = string.Empty;

        [Required, MaxLength(150), EmailAddress]
        public string Correo { get; set; } = string.Empty;     // único

        [MaxLength(20)]
        public string Telefono { get; set; } = string.Empty;

        [MaxLength(30)]
        public string Rol { get; set; } = "Admin";

        public bool Activo { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }
}
