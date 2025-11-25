using Gym_FitByte.Data;
using Gym_FitByte.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
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

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.WriteIndented = true;
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
// MIDDLEWARES
// ================================
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors("NuevaPolitica");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// ================================
// APLICAR MIGRACIONES AUTOMÁTICAS (OPCIONAL PERO ÚTIL)
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

app.Run();
