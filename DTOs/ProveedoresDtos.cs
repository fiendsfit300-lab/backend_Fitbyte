namespace Gym_FitByte.DTOs
{
    public class CrearProveedorDto
    {
        public string NombreEmpresa { get; set; } = string.Empty;
        public string PersonaContacto { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string RFC { get; set; } = string.Empty;
    }

    public class ActualizarProveedorDto : CrearProveedorDto
    {
        public bool Activo { get; set; } = true;
    }
}
