namespace Gym_FitByte.Models
{
    public class MovimientoCaja
    {
        public int Id { get; set; }
        public int CorteCajaId { get; set; }
        public CorteCaja CorteCaja { get; set; }

        public string Tipo { get; set; } // Venta, Visita, Membresia, Renovacion, Compra
        public decimal Monto { get; set; } // compras negativas
        public string Descripcion { get; set; }
        public DateTime Fecha { get; set; }
    }

}
