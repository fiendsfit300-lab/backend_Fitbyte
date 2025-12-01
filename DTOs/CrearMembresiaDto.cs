// ============================================================
// DTOs NECESARIOS PARA EL CONTROLADOR
// ============================================================

public class CrearMembresiaDto
{
    public string Nombre { get; set; } = string.Empty;
    public int Edad { get; set; }
    public string Telefono { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Rutina { get; set; } = string.Empty;
    public string EnfermedadesOLesiones { get; set; } = "Ninguna";
    public IFormFile? Foto { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.Now;
    public DateTime FechaVencimiento { get; set; }
    public string FormaPago { get; set; } = "Efectivo";
    public string Tipo { get; set; } = "Inscripción";
    public decimal MontoPagado { get; set; }
    public string Nivel { get; set; } = "Básica";
}

public class RenovarMembresiaDto
{
    public DateTime NuevaFechaVencimiento { get; set; }
    public string TipoPago { get; set; } = "Efectivo";
    public decimal MontoPagado { get; set; }
}

public class EditarMembresiaDto
{
    public string Nombre { get; set; } = string.Empty;
    public int Edad { get; set; }
    public string Telefono { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;

    public string Rutina { get; set; } = string.Empty;
    public string EnfermedadesOLesiones { get; set; } = "Ninguna";

    public string FormaPago { get; set; } = "Efectivo";
    public string Tipo { get; set; } = "Inscripción";
    public decimal MontoPagado { get; set; }

    public DateTime FechaVencimiento { get; set; }

    public string Nivel { get; set; } = "Básica";

    public IFormFile? Foto { get; set; }
}
