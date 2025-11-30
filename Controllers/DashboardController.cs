using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gym_FitByte.Data;

namespace Gym_FitByte.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;
        public DashboardController (AppDbContext context) => _context = context;

      
        [HttpGet("resumen-general")]
        public async Task<IActionResult> ResumenGeneral()
        {
            var hoy = DateTime.Today;
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

        
            var totalMembresias = await _context.Membresias.CountAsync();
            var activas = await _context.Membresias.CountAsync(m => m.Activa && m.FechaVencimiento >= hoy);
            var vencidas = await _context.Membresias.CountAsync(m => m.FechaVencimiento < hoy);
            var nuevasMes = await _context.Membresias.CountAsync(m => m.FechaRegistro >= inicioMes);

           
            var porVencer = await _context.Membresias
                .CountAsync(m =>
                    m.Activa &&
                    m.FechaVencimiento >= hoy &&
                    m.FechaVencimiento <= hoy.AddDays(3)
                );

        
            var asistenciasHoy = await _context.Asistencias
                .CountAsync(a => a.FechaHora.Date == hoy);

        
            var visitasHoy = await _context.VisitasDiarias
                .CountAsync(v => v.FechaHoraIngreso.Date == hoy && v.Estado != Models.EstadoVisita.Cancelada);
 
            var ventasHoy = await _context.Ventas
                .Where(v => v.FechaVenta.Date == hoy)
                .SumAsync(v => v.Total);

            var ventasMes = await _context.Ventas
                .Where(v => v.FechaVenta >= inicioMes)
                .SumAsync(v => v.Total);
 
            var ventasVisitasHoy = await _context.VentasVisitas
                .Where(v => v.FechaVenta.Date == hoy)
                .SumAsync(v => v.Costo);

            var ventasVisitasMes = await _context.VentasVisitas
                .Where(v => v.FechaVenta >= inicioMes)
                .SumAsync(v => v.Costo);
 
            var comprasHoy = await _context.Compras
                .Where(c => c.FechaCompra.Date == hoy)
                .SumAsync(c => c.Total);

            var comprasMes = await _context.Compras
                .Where(c => c.FechaCompra >= inicioMes)
                .SumAsync(c => c.Total);

       
            var stockBajo = await _context.Inventario
                .Include(i => i.Producto)
                .CountAsync(i => i.Cantidad <= 5 && i.Producto!.Activo);

            var valorInventario = await _context.Inventario
                .Include(i => i.Producto)
                .Where(i => i.Producto!.Activo)
                .SumAsync(i => i.Cantidad * i.Producto.PrecioFinal);

         
            var preregPendientes = await _context.PreRegistros
                .CountAsync(p => p.Estado == Models.EstadoPreRegistro.Pendiente);
 
            return Ok(new
            {
                miembros = new
                {
                    total = totalMembresias,
                    activas,
                    vencidas,
                    porVencer,
                    nuevasMes
                },

                asistencias = new
                {
                    hoy = asistenciasHoy
                },

                visitas = new
                {
                    hoy = visitasHoy
                },

                ventas = new
                {
                    productosHoy = ventasHoy,
                    productosMes = ventasMes,
                    visitasHoy = ventasVisitasHoy,
                    visitasMes = ventasVisitasMes
                },

                compras = new
                {
                    hoy = comprasHoy,
                    mes = comprasMes
                },

                inventario = new
                {
                    stockBajo,
                    valorInventario
                },

                preregistros = new
                {
                    pendientes = preregPendientes
                }
            });
        }

  
        [HttpGet("ingresos-mensuales")]
        public async Task<IActionResult> IngresosMensuales()
        {
            var year = DateTime.Now.Year;

            var ventasProductos = await _context.Ventas
                .Where(v => v.FechaVenta.Year == year)
                .GroupBy(v => v.FechaVenta.Month)
                .Select(g => new { mes = g.Key, total = g.Sum(x => x.Total) })
                .ToListAsync();

            var visitas = await _context.VentasVisitas
                .Where(v => v.FechaVenta.Year == year)
                .GroupBy(v => v.FechaVenta.Month)
                .Select(g => new { mes = g.Key, total = g.Sum(x => x.Costo) })
                .ToListAsync();

            return Ok(new { ventasProductos, visitas });
        }

  
      
        [HttpGet("asistencias-semana")]
        public async Task<IActionResult> AsistenciasSemana()
        {
            var semana = DateTime.Today.AddDays(-7);

            var datos = await _context.Asistencias
                .Where(a => a.FechaHora >= semana)
                .GroupBy(a => a.FechaHora.DayOfWeek)
                .Select(g => new { dia = g.Key.ToString(), total = g.Count() })
                .ToListAsync();

            return Ok(datos);
        }
 
        [HttpGet("top-productos")]
        public async Task<IActionResult> TopProductos()
        {
            var datos = await _context.Ventas
                .Include(v => v.Items)
                .SelectMany(v => v.Items)
                .GroupBy(i => i.ProductoId)
                .Select(g => new
                {
                    productoId = g.Key,
                    cantidad = g.Sum(x => x.Cantidad)
                })
                .OrderByDescending(x => x.cantidad)
                .Take(5)
                .ToListAsync();

            return Ok(datos);
        }
        

      
        [HttpGet("membresias-por-vencer")]
        public async Task<IActionResult> MembresiasPorVencer()
        {
            var hoy = DateTime.Today;
            var limite = hoy.AddDays(3);

            var data = await _context.Membresias
                .Where(m =>
                    m.Activa &&
                    m.FechaVencimiento >= hoy &&
                    m.FechaVencimiento <= limite
                )
                .Select(m => new
                {
                    m.Id,
                    m.Nombre,
                    m.CodigoCliente,
                    m.FechaVencimiento,
                    m.FotoUrl
                })
                .ToListAsync();

            return Ok(data);
        }
    }
}
