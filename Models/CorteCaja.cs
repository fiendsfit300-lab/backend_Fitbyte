namespace Gym_FitByte.Models
{
    public class CorteCaja
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        public decimal CajaInicial { get; set; }
        public decimal IngresosTotales { get; set; }
        public decimal EgresosTotales { get; set; }
        public decimal CajaFinal { get; set; }

        public decimal VentasProductos { get; set; }
        public decimal VentasVisitas { get; set; }
        public decimal MembresiasNuevas { get; set; }
        public decimal Renovaciones { get; set; }

        public decimal Compras { get; set; }

        public decimal Ganancia { get; set; }

        public string Usuario { get; set; } = string.Empty;
    }

}
