using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gym_FitByte.Data;
using Gym_FitByte.Models;

namespace Gym_FitByte.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CorteCajaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CorteCajaController(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // 1. CORTE DE CAJA DIARIO (SIN GUARDAR)
        // ============================================================
        [HttpGet("diario")]
        public async Task<IActionResult> CorteDiario([FromQuery] decimal cajaInicial = 0)
        {
            var hoy = DateTime.Today;

            // ===================================
            // INGRESOS
            // ===================================

            // Ventas de productos
            var ventasProductos = await _context.Ventas
                .Where(v => v.FechaVenta.Date == hoy)
                .SumAsync(v => v.Total);

            var cantidadVentasProductos = await _context.Ventas
                .CountAsync(v => v.FechaVenta.Date == hoy);

            // Ventas de visitas
            var ventasVisitas = await _context.VentasVisitas
                .Where(v => v.FechaVenta.Date == hoy)
                .SumAsync(v => v.Costo);

            var cantidadVentasVisitas = await _context.VentasVisitas
                .CountAsync(v => v.FechaVenta.Date == hoy);

            // Membresías nuevas (FechaRegistro)
            var ingresosMembresiasNuevas = await _context.Membresias
                .Where(m => m.FechaRegistro.Date == hoy)
                .SumAsync(m => m.MontoPagado);

            var cantidadMembresiasNuevas = await _context.Membresias
                .CountAsync(m => m.FechaRegistro.Date == hoy);

            // Renovaciones (si FechaVencimiento cambia hoy)
            var ingresosRenovaciones = await _context.Membresias
                .Where(m => m.FechaVencimiento.Date == hoy)
                .SumAsync(m => m.MontoPagado);

            var cantidadRenovaciones = await _context.Membresias
                .CountAsync(m => m.FechaVencimiento.Date == hoy);

            // ===================================
            // EGRESOS
            // ===================================
            var comprasHoy = await _context.Compras
                .Where(c => c.FechaCompra.Date == hoy)
                .SumAsync(c => c.Total);

            // ===================================
            // TOTALES
            // ===================================
            var ingresosTotales =
                ventasProductos +
                ventasVisitas +
                ingresosMembresiasNuevas +
                ingresosRenovaciones;

            var egresosTotales = comprasHoy;

            var cajaFinal = cajaInicial + ingresosTotales - egresosTotales;
            var ganancia = ingresosTotales - egresosTotales;

            return Ok(new
            {
                fecha = hoy.ToString("yyyy-MM-dd"),

                cajaInicial,

                ingresos = new
                {
                    ventasProductos,
                    cantidadVentasProductos,

                    ventasVisitas,
                    cantidadVentasVisitas,

                    ingresosMembresiasNuevas,
                    cantidadMembresiasNuevas,

                    ingresosRenovaciones,
                    cantidadRenovaciones,

                    ingresosTotales
                },

                egresos = new
                {
                    comprasHoy,
                    egresosTotales
                },

                resultados = new
                {
                    cajaFinal,
                    ganancia
                }
            });
        }

        // ============================================================
        // 2. GUARDAR CORTE DEL DÍA (CIERRE DE CAJA)
        // ============================================================
        [HttpPost("cerrar")]
        public async Task<IActionResult> GuardarCorte([FromBody] CorteCaja dto)
        {
            dto.Ganancia = dto.IngresosTotales - dto.EgresosTotales;
            dto.CajaFinal = dto.CajaInicial + dto.Ganancia;

            _context.CortesCaja.Add(dto);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Corte de caja guardado correctamente",
                id = dto.Id
            });
        }

        // ============================================================
        // 3. HISTORIAL POR DÍA
        // ============================================================
        [HttpGet("historial/dia")]
        public async Task<IActionResult> CortesPorDia([FromQuery] DateTime fecha)
        {
            var cortes = await _context.CortesCaja
                .Where(c => c.Fecha.Date == fecha.Date)
                .OrderBy(c => c.Fecha)
                .ToListAsync();

            return Ok(cortes);
        }

        // ============================================================
        // 4. HISTORIAL SEMANAL
        // ============================================================
        [HttpGet("historial/semana")]
        public async Task<IActionResult> CortesSemana()
        {
            var hoy = DateTime.Today;
            var inicioSemana = hoy.AddDays(-(int)hoy.DayOfWeek + 1);

            var cortes = await _context.CortesCaja
                .Where(c => c.Fecha.Date >= inicioSemana)
                .OrderBy(c => c.Fecha)
                .ToListAsync();

            return Ok(cortes);
        }

        // ============================================================
        // 5. HISTORIAL MENSUAL
        // ============================================================
        [HttpGet("historial/mes")]
        public async Task<IActionResult> CortesMes([FromQuery] int year, [FromQuery] int month)
        {
            var cortes = await _context.CortesCaja
                .Where(c => c.Fecha.Year == year && c.Fecha.Month == month)
                .OrderBy(c => c.Fecha)
                .ToListAsync();

            return Ok(cortes);
        }
    }
}
