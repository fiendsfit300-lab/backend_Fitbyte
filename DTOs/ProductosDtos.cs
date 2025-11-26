using Microsoft.AspNetCore.Mvc;

namespace Gym_FitByte.DTOs
{
    public class CrearProductoDto
    {
        [FromForm] public int ProveedorId { get; set; }
        [FromForm] public string Nombre { get; set; } = string.Empty;

        // 🔹 Precio del paquete (costo)
        [FromForm] public decimal Precio { get; set; }

        [FromForm] public string Categoria { get; set; } = string.Empty;

        // 🔹 Cuántas piezas tiene un paquete
        [FromForm] public int PiezasPorPaquete { get; set; }

        // 🔥 Precio final de venta por pieza
        [FromForm] public decimal PrecioFinal { get; set; }

        [FromForm] public IFormFile? Foto { get; set; }
    }

    public class ActualizarProductoDto
    {
        [FromForm] public string Nombre { get; set; } = string.Empty;

        // 🔹 Precio del paquete (costo)
        [FromForm] public decimal Precio { get; set; }

        [FromForm] public string Categoria { get; set; } = string.Empty;
        [FromForm] public bool Activo { get; set; } = true;

        // 🔹 Piezas en el paquete
        [FromForm] public int PiezasPorPaquete { get; set; }

        // 🔥 Precio final de venta por pieza
        [FromForm] public decimal PrecioFinal { get; set; }

        [FromForm] public IFormFile? Foto { get; set; }
    }

    public class CrearProductosMultiplesDto
    {
        [FromForm] public int ProveedorId { get; set; }
        [FromForm] public List<string> Nombre { get; set; } = new();
        [FromForm] public List<decimal> Precio { get; set; } = new();
        [FromForm] public List<string> Categoria { get; set; } = new();
        [FromForm] public List<int> PiezasPorPaquete { get; set; } = new();
        [FromForm] public List<IFormFile?> Foto { get; set; } = new();

        // 🔥 Precio final por pieza para cada producto (opcional)
        [FromForm] public List<decimal> PrecioFinal { get; set; } = new();
    }
}
