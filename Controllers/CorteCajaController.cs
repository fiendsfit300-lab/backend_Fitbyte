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
        // 1. CORTE DE CAJA (PREVISUALIZACIÓN DEL PERIODO)
        //    Usa el último corte del día como inicio del periodo
        // ============================================================
        [HttpGet("diario")]
        public async Task<IActionResult> CorteDiario([FromQuery] decimal cajaInicial = 0)
        {
            var hoy = DateTime.Today;
            var ahora = DateTime.Now;

            // 1) Buscar el último corte registrado HOY
            var ultimoCorte = await _context.CortesCaja
                .Where(c => c.Fecha.Date == hoy)
                .OrderByDescending(c => c.Fecha)
                .FirstOrDefaultAsync();

            DateTime inicioPeriodo;
            decimal cajaInicialReal;

            if (ultimoCorte == null)
            {
                // PRIMER CORTE DEL DÍA
                inicioPeriodo = hoy; // 00:00
                cajaInicialReal = cajaInicial;   // lo que pone el usuario
            }
            else
            {
                // CORTES POSTERIORES
                inicioPeriodo = ultimoCorte.Fecha;    // desde el último corte
                cajaInicialReal = ultimoCorte.CajaFinal; // caja arranca con lo que quedó
            }

            // ===================================
            // INGRESOS (SOLO DEL PERIODO)
            // ===================================

            // Ventas de productos
            var ventasProductos = await _context.Ventas
                .Where(v => v.FechaVenta >= inicioPeriodo && v.FechaVenta <= ahora)
                .SumAsync(v => v.Total);

            var cantidadVentasProductos = await _context.Ventas
                .CountAsync(v => v.FechaVenta >= inicioPeriodo && v.FechaVenta <= ahora);

            // Ventas de visitas
            var ventasVisitas = await _context.VentasVisitas
                .Where(v => v.FechaVenta >= inicioPeriodo && v.FechaVenta <= ahora)
                .SumAsync(v => v.Costo);

            var cantidadVentasVisitas = await _context.VentasVisitas
                .CountAsync(v => v.FechaVenta >= inicioPeriodo && v.FechaVenta <= ahora);

            // Membresías nuevas (FechaRegistro)
            var ingresosMembresiasNuevas = await _context.Membresias
                .Where(m => m.FechaRegistro >= inicioPeriodo && m.FechaRegistro <= ahora)
                .SumAsync(m => m.MontoPagado);

            var cantidadMembresiasNuevas = await _context.Membresias
                .CountAsync(m => m.FechaRegistro >= inicioPeriodo && m.FechaRegistro <= ahora);

            // Renovaciones (FechaVencimiento en el periodo)
            var ingresosRenovaciones = await _context.Membresias
                .Where(m => m.FechaVencimiento >= inicioPeriodo && m.FechaVencimiento <= ahora)
                .SumAsync(m => m.MontoPagado);

            var cantidadRenovaciones = await _context.Membresias
                .CountAsync(m => m.FechaVencimiento >= inicioPeriodo && m.FechaVencimiento <= ahora);

            // ===================================
            // EGRESOS (SOLO DEL PERIODO)
            // ===================================
            var comprasHoy = await _context.Compras
                .Where(c => c.FechaCompra >= inicioPeriodo && c.FechaCompra <= ahora)
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

            var cajaFinal = cajaInicialReal + ingresosTotales - egresosTotales;
            var ganancia = ingresosTotales - egresosTotales;

            return Ok(new
            {
                fecha = ahora,
                inicioPeriodo,
                cajaInicial = cajaInicialReal,

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
        // 2. GUARDAR CORTE DEL PERIODO (CIERRE DE CAJA)
        // ============================================================
        [HttpPost("cerrar")]
        public async Task<IActionResult> GuardarCorte([FromBody] CorteCaja dto)
        {
            // La fecha del corte la pone el servidor (momento de cierre)
            dto.Fecha = DateTime.Now;

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

        // 4. HISTORIAL SEMANAL
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

        // 5. HISTORIAL MENSUAL
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
