namespace Gym_FitByte.Models
{
    public class CorteCaja
    {
        public int Id { get; set; }
        public DateTime FechaApertura { get; set; }
        public DateTime? FechaCierre { get; set; }
        public decimal MontoInicial { get; set; }
        public decimal? MontoFinal { get; set; }
        public int Estado { get; set; } // 0 = abierto, 1 = cerrado

        public List<MovimientoCaja> Movimientos { get; set; } = new();
    }


}
