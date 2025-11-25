using Microsoft.AspNetCore.Mvc;

namespace Gym_FitByte.DTOs
{
    public class CrearProductoDto
    {
        [FromForm] public int ProveedorId { get; set; }
        [FromForm] public string Nombre { get; set; } = string.Empty;
        [FromForm] public decimal Precio { get; set; }
        [FromForm] public string Categoria { get; set; } = string.Empty;

        // Cuántas piezas tiene un paquete
        [FromForm] public int PiezasPorPaquete { get; set; }

        [FromForm] public IFormFile? Foto { get; set; }
    }

    public class ActualizarProductoDto
    {
        [FromForm] public string Nombre { get; set; } = string.Empty;
        [FromForm] public decimal Precio { get; set; }
        [FromForm] public string Categoria { get; set; } = string.Empty;
        [FromForm] public bool Activo { get; set; } = true;
        [FromForm] public int PiezasPorPaquete { get; set; }
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
    }
}
