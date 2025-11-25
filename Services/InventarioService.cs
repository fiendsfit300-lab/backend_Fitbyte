using Microsoft.EntityFrameworkCore;
using Gym_FitByte.Data;
using Gym_FitByte.Models;

namespace Gym_FitByte.Services
{
    public class InventarioService : IInventarioService
    {
        private readonly AppDbContext _context;

        public InventarioService(AppDbContext context)
        {
            _context = context;
        }

        // ================================
        // COMPRA → AUMENTA INVENTARIO
        // ================================
        // IMPORTANTE: Ya NO abre transacción.
        // Si necesitas transacción, la controla el Controller.
        public async Task<bool> ActualizarInventarioCompra(int compraId)
        {
            var compra = await _context.Compras
                .Include(c => c.Items)
                    .ThenInclude(i => i.Producto) // para leer PiezasPorPaquete
                .FirstOrDefaultAsync(c => c.Id == compraId);

            if (compra == null) return false;

            foreach (var item in compra.Items)
            {
                if (item.Producto == null)
                    continue;

                // piezas por paquete definidas en Producto.PiezasPorPaquete
                var piezasPorPaquete = item.Producto.PiezasPorPaquete;

                if (piezasPorPaquete <= 0)
                    piezasPorPaquete = 1;

                // Cantidad total de piezas que entran al inventario
                var piezasTotales = item.Cantidad * piezasPorPaquete;

                // Buscar o crear registro de inventario
                var inventario = await _context.Inventario
                    .FirstOrDefaultAsync(i => i.ProductoId == item.ProductoId);

                if (inventario == null)
                {
                    inventario = new Inventario
                    {
                        ProductoId = item.ProductoId,
                        Cantidad = piezasTotales,
                        CantidadComprada = piezasTotales,
                        CantidadVendida = 0,
                        FechaActualizacion = DateTime.Now
                    };
                    _context.Inventario.Add(inventario);
                }
                else
                {
                    inventario.Cantidad += piezasTotales;
                    inventario.CantidadComprada += piezasTotales;
                    inventario.FechaActualizacion = DateTime.Now;
                }

                // Historial de movimientos (Entrada por compra)
                var movimiento = new HistorialMovimiento
                {
                    ProductoId = item.ProductoId,
                    Tipo = TipoMovimiento.Entrada,
                    Cantidad = piezasTotales,
                    Motivo = "Compra",
                    CompraId = compraId,
                    PrecioUnitario = item.PrecioUnitario,
                    Referencia = compra.Folio,
                    FechaMovimiento = DateTime.Now
                };
                _context.HistorialMovimientos.Add(movimiento);

                // Marcar item como procesado en inventario
                item.InventarioActualizado = true;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // ================================
        // VENTA → DISMINUYE INVENTARIO
        // ================================
        public async Task<bool> ActualizarInventarioVenta(int ventaId)
        {
            var venta = await _context.Ventas
                .Include(v => v.Items)
                    .ThenInclude(i => i.Producto)
                .FirstOrDefaultAsync(v => v.Id == ventaId);

            if (venta == null) return false;

            foreach (var item in venta.Items)
            {
                var inventario = await _context.Inventario
                    .FirstOrDefaultAsync(i => i.ProductoId == item.ProductoId);

                if (inventario == null || inventario.Cantidad < item.Cantidad)
                {
                    throw new InvalidOperationException(
                        $"Stock insuficiente para el producto {item.Producto?.Nombre}"
                    );
                }

                // La venta es en PIEZAS, restamos directo
                inventario.Cantidad -= item.Cantidad;
                inventario.CantidadVendida += item.Cantidad;
                inventario.FechaActualizacion = DateTime.Now;

                var movimiento = new HistorialMovimiento
                {
                    ProductoId = item.ProductoId,
                    Tipo = TipoMovimiento.Salida,
                    Cantidad = item.Cantidad,
                    Motivo = "Venta",
                    VentaId = ventaId,
                    PrecioUnitario = item.PrecioUnitario,
                    FechaMovimiento = DateTime.Now
                };
                _context.HistorialMovimientos.Add(movimiento);

                item.InventarioActualizado = true;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // ================================
        // REVERTIR COMPRA
        // ================================
        public async Task<bool> RevertirInventarioCompra(int compraId)
        {
            var compra = await _context.Compras
                .Include(c => c.Items)
                    .ThenInclude(i => i.Producto)
                .FirstOrDefaultAsync(c => c.Id == compraId);

            if (compra == null) return false;

            foreach (var item in compra.Items.Where(i => i.InventarioActualizado))
            {
                if (item.Producto == null)
                    continue;

                var piezasPorPaquete = item.Producto.PiezasPorPaquete;
                if (piezasPorPaquete <= 0) piezasPorPaquete = 1;
                var piezasTotales = item.Cantidad * piezasPorPaquete;

                var inventario = await _context.Inventario
                    .FirstOrDefaultAsync(i => i.ProductoId == item.ProductoId);

                if (inventario != null)
                {
                    inventario.Cantidad -= piezasTotales;
                    inventario.CantidadComprada -= piezasTotales;
                    inventario.FechaActualizacion = DateTime.Now;
                }

                var movimiento = new HistorialMovimiento
                {
                    ProductoId = item.ProductoId,
                    Tipo = TipoMovimiento.Salida,
                    Cantidad = piezasTotales,
                    Motivo = "Reversión de compra",
                    CompraId = compraId,
                    PrecioUnitario = item.PrecioUnitario,
                    Referencia = $"REV-{compra.Folio}",
                    FechaMovimiento = DateTime.Now
                };
                _context.HistorialMovimientos.Add(movimiento);

                item.InventarioActualizado = false;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // ================================
        // REVERTIR VENTA
        // ================================
        public async Task<bool> RevertirInventarioVenta(int ventaId)
        {
            var venta = await _context.Ventas
                .Include(v => v.Items)
                .FirstOrDefaultAsync(v => v.Id == ventaId);

            if (venta == null) return false;

            foreach (var item in venta.Items.Where(i => i.InventarioActualizado))
            {
                var inventario = await _context.Inventario
                    .FirstOrDefaultAsync(i => i.ProductoId == item.ProductoId);

                if (inventario != null)
                {
                    inventario.Cantidad += item.Cantidad;
                    inventario.CantidadVendida -= item.Cantidad;
                    inventario.FechaActualizacion = DateTime.Now;
                }

                var movimiento = new HistorialMovimiento
                {
                    ProductoId = item.ProductoId,
                    Tipo = TipoMovimiento.Entrada,
                    Cantidad = item.Cantidad,
                    Motivo = "Reversión de venta",
                    VentaId = ventaId,
                    PrecioUnitario = item.PrecioUnitario,
                    FechaMovimiento = DateTime.Now
                };
                _context.HistorialMovimientos.Add(movimiento);

                item.InventarioActualizado = false;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // ================================
        // STOCK ACTUAL
        // ================================
        public async Task<int> ObtenerStockActual(int productoId)
        {
            var inventario = await _context.Inventario
                .FirstOrDefaultAsync(i => i.ProductoId == productoId);

            return inventario?.Cantidad ?? 0;
        }

        // ================================
        // AJUSTE MANUAL
        // ================================
        public async Task<bool> AjustarInventario(int productoId, int cantidad, string motivo, string? referencia = null)
        {
            var inventario = await _context.Inventario
                .FirstOrDefaultAsync(i => i.ProductoId == productoId);

            if (inventario == null)
            {
                inventario = new Inventario
                {
                    ProductoId = productoId,
                    Cantidad = cantidad,
                    CantidadComprada = 0,
                    CantidadVendida = 0,
                    FechaActualizacion = DateTime.Now
                };
                _context.Inventario.Add(inventario);
            }
            else
            {
                inventario.Cantidad = cantidad;
                inventario.FechaActualizacion = DateTime.Now;
            }

            var movimiento = new HistorialMovimiento
            {
                ProductoId = productoId,
                Tipo = TipoMovimiento.Ajuste,
                Cantidad = cantidad,
                Motivo = motivo,
                Referencia = referencia,
                FechaMovimiento = DateTime.Now
            };
            _context.HistorialMovimientos.Add(movimiento);

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
