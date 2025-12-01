using Gym_FitByte.Data;
using Gym_FitByte.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Globalization;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ================================
// CONFIGURACIÓN DE DB
// ================================
builder.Configuration.AddEnvironmentVariables();

var connectionString =
    builder.Configuration.GetConnectionString("MySqlConnection")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 39)))
);

// ================================
// SERVICIOS
// ================================
builder.Services.AddScoped<IInventarioService, InventarioService>();

// ================================
// CONTROLADORES + CONFIG JSON
// ================================
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.WriteIndented = true;

    // 🔥 ESTA LÍNEA ES CRÍTICA PARA QUE EL BACKEND ACEPTE minúsculas/mayúsculas
    // Y EVITE QUE LLEGUEN VALORES COMO 0 EN DTOs
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});

// ================================
// CORS
// ================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("NuevaPolitica", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ================================
// SWAGGER
// ================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Gym FitByte API",
        Version = "v1"
    });
});

var app = builder.Build();

// ================================
// SWAGGER
// ================================
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ================================
// RUTEO
// ================================
app.UseRouting();

// ================================
// CULTURA Y ZONA HORARIA MX
// ================================
app.Use(async (context, next) =>
{
    // Cultura general MX
    var culturaMx = new CultureInfo("es-MX");
    Thread.CurrentThread.CurrentCulture = culturaMx;
    Thread.CurrentThread.CurrentUICulture = culturaMx;

    // Zona horaria (Linux + Windows)
    TimeZoneInfo tz;

    try
    {
        // Linux - Render
        tz = TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City");
    }
    catch
    {
        // Windows - Desarrollo
        tz = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
    }

    context.Items["AhoraMx"] = TimeZoneInfo.ConvertTime(DateTime.UtcNow, tz);

    await next();
});

// ================================
// MIDDLEWARES
// ================================
app.UseCors("NuevaPolitica");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// ================================
// MIGRACIONES AUTOMÁTICAS
// ================================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        db.Database.Migrate();
        Console.WriteLine("✔ Migraciones aplicadas.");
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Error aplicando migraciones: " + ex.Message);
    }
}

// ================================
app.Run();
