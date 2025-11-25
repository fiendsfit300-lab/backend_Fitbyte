namespace Gym_FitByte.DTOs
{
    public class AdminCrearDto
    {
        public string Usuario { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string Rol { get; set; } = "Admin";
    }

    public class AdminActualizarDto
    {
        public string? Usuario { get; set; }
        public string? Contrasena { get; set; }
        public string? Nombre { get; set; }
        public string? Correo { get; set; }
        public string? Telefono { get; set; }
        public string? Rol { get; set; }
        public bool? Activo { get; set; }
    }
}
