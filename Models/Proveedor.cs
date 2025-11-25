using System.ComponentModel.DataAnnotations;

namespace Gym_FitByte.Models
{
    public class Proveedor
    {
        [Key] public int Id { get; set; }

        [Required, MaxLength(150)]
        public string NombreEmpresa { get; set; } = string.Empty;

        [MaxLength(120)]
        public string PersonaContacto { get; set; } = string.Empty;

        [MaxLength(30)]
        public string Telefono { get; set; } = string.Empty;

        [MaxLength(120), EmailAddress]
        public string Email { get; set; } = string.Empty;

        [MaxLength(250)]
        public string Direccion { get; set; } = string.Empty;

        [Required, MaxLength(13)]
        public string RFC { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        public ICollection<Producto>? Productos { get; set; }
    }
}
