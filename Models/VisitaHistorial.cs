using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_FitByte.Models
{
    public class VisitaHistorial
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Visita")]
        public int VisitaId { get; set; }
        public VisitaDiaria? Visita { get; set; }

        [Required, MaxLength(40)]
        public string Accion { get; set; } = "Creado"; // Creado / Actualizado / Cancelado / Salida

        public DateTime Fecha { get; set; } = DateTime.Now;

        [MaxLength(250)]
        public string? Detalle { get; set; }
    }
}
