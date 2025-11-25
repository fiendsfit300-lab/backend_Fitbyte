namespace Gym_FitByte.Services
{
    public interface IInventarioService
    {
        Task<bool> ActualizarInventarioCompra(int compraId);
        Task<bool> ActualizarInventarioVenta(int ventaId);
        Task<int> ObtenerStockActual(int productoId);
        Task<bool> AjustarInventario(int productoId, int cantidad, string motivo, string? referencia = null);
    }
}
