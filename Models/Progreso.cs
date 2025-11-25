using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_FitByte.Models
{
    public class Progreso
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string CodigoCliente { get; set; } = string.Empty;

        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        public double Peso { get; set; }
        public double Pecho { get; set; }
        public double Cintura { get; set; }
        public double Brazo { get; set; }
        public double Pierna { get; set; }
        public double Hombros { get; set; }

        public string? FotoUrl { get; set; }

        [ForeignKey("Membresia")]
        public int? MembresiaId { get; set; }
        public Membresia? Membresia { get; set; }
    }
}
