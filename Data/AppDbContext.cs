using Gym_FitByte.Models;
using Microsoft.EntityFrameworkCore;

namespace Gym_FitByte.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // ====== Tablas existentes ======
        public DbSet<Membresia> Membresias { get; set; }
        public DbSet<MembresiaHistorial> MembresiasHistorial { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<VentaVisita> VentasVisitas { get; set; }
        public DbSet<Asistencia> Asistencias { get; set; }
        public DbSet<Progreso> Progresos { get; set; }

        // ====== RUTINAS (Nuevo) ======
        public DbSet<Rutina> Rutinas { get; set; }
        public DbSet<Ejercicio> Ejercicios { get; set; }


        // ====== Nuevas tablas (Proveedores / Productos / Compras / Ventas) ======
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Compra> Compras { get; set; }
        public DbSet<CompraItem> ComprasItems { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<VentaItem> VentasItems { get; set; }

        // ====== Pre-Registros ======
        public DbSet<PreRegistro> PreRegistros { get; set; }

        // ====== Visitas de un día ======
        public DbSet<VisitaDiaria> VisitasDiarias { get; set; }
        public DbSet<VisitaHistorial> VisitasHistorial { get; set; }

        // ====== Inventario e Historial ======
        public DbSet<Inventario> Inventario { get; set; }
        public DbSet<HistorialMovimiento> HistorialMovimientos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ---- Membresías ----
            modelBuilder.Entity<Membresia>()
                .HasIndex(m => m.CodigoCliente)
                .IsUnique();

            modelBuilder.Entity<MembresiaHistorial>()
                .HasOne(h => h.Membresia)
                .WithMany(m => m.Historial!)
                .HasForeignKey(h => h.MembresiaId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---- Proveedores ----
            modelBuilder.Entity<Proveedor>()
                .HasIndex(p => p.RFC)
                .IsUnique();

            // ---- Productos ----
            modelBuilder.Entity<Producto>()
                .HasIndex(p => new { p.ProveedorId, p.Nombre })
                .IsUnique();

            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Proveedor)
                .WithMany(pr => pr.Productos)
                .HasForeignKey(p => p.ProveedorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Ejercicio>()
             .HasOne(e => e.Rutina)
             .WithMany(r => r.Ejercicios)
             .HasForeignKey(e => e.RutinaId)
             .OnDelete(DeleteBehavior.Cascade);
            // ---- Compras ----
            modelBuilder.Entity<Compra>()
                .HasMany(c => c.Items)
                .WithOne(i => i.Compra!)
                .HasForeignKey(i => i.CompraId)
                .OnDelete(DeleteBehavior.Cascade);

            // ⚠ Ya no configuramos Proveedor en CompraItem porque NO existe esa propiedad.

            // ---- Ventas ----
            modelBuilder.Entity<Venta>()
                .HasMany(v => v.Items)
                .WithOne(i => i.Venta!)
                .HasForeignKey(i => i.VentaId)
                .OnDelete(DeleteBehavior.Cascade);

            // ---- Pre-Registros ----
            modelBuilder.Entity<PreRegistro>()
                .Property(p => p.Estado)
                .HasConversion<int>();

            // ---- Visitas de un día ----
            modelBuilder.Entity<VisitaDiaria>()
                .HasMany(v => v.Historial)
                .WithOne(h => h.Visita!)
                .HasForeignKey(h => h.VisitaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VisitaDiaria>()
                .HasIndex(v => v.FechaHoraIngreso);

            modelBuilder.Entity<VisitaDiaria>()
                .Property(v => v.Estado)
                .HasConversion<int>();

            modelBuilder.Entity<VisitaHistorial>()
                .HasIndex(h => new { h.VisitaId, h.Fecha });

            // ---- Inventario ----
            modelBuilder.Entity<Inventario>()
                .HasIndex(i => i.ProductoId)
                .IsUnique();

            modelBuilder.Entity<Inventario>()
                .HasOne(i => i.Producto)
                .WithOne()
                .HasForeignKey<Inventario>(i => i.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);

            // ---- Historial de Movimientos ----
            modelBuilder.Entity<HistorialMovimiento>()
                .HasIndex(h => h.ProductoId);

            modelBuilder.Entity<HistorialMovimiento>()
                .HasIndex(h => h.FechaMovimiento);

            modelBuilder.Entity<HistorialMovimiento>()
                .HasIndex(h => h.CompraId);

            modelBuilder.Entity<HistorialMovimiento>()
                .HasIndex(h => h.VentaId);

            modelBuilder.Entity<HistorialMovimiento>()
                .Property(h => h.Tipo)
                .HasConversion<int>();

            modelBuilder.Entity<HistorialMovimiento>()
                .HasOne(h => h.Producto)
                .WithMany()
                .HasForeignKey(h => h.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);


            // ========================================
            //             RUTINAS NUEVO
            // ========================================

            // Una rutina tiene muchos ejercicios
            modelBuilder.Entity<Rutina>()
                .HasMany(r => r.Ejercicios)
                .WithOne(e => e.Rutina!)
                .HasForeignKey(e => e.RutinaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices recomendados
            modelBuilder.Entity<Rutina>()
                .HasIndex(r => new { r.Nivel, r.Genero });

            modelBuilder.Entity<Rutina>()
                .Property(r => r.Nivel)
                .HasMaxLength(30);

            modelBuilder.Entity<Rutina>()
                .Property(r => r.Genero)
                .HasMaxLength(10);
        }
    }
}
