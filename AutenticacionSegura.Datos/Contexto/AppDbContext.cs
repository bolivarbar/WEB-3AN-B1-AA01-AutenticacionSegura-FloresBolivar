using AutenticacionSegura.Datos.Entidades;            // Para usar la clase Usuario
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // IdentityDbContext
using Microsoft.EntityFrameworkCore;                   // DbContext, ModelBuilder

namespace AutenticacionSegura.Datos.Contexto;

/// <summary>
/// Contexto de base de datos de la aplicación. Es el puente entre las
/// clases de C# y las tablas de SQL Server (Entity Framework Core).
/// Al heredar de IdentityDbContext&lt;Usuario&gt;, EF Core crea
/// automáticamente las tablas de Identity: AspNetUsers (usuarios),
/// AspNetRoles (roles), AspNetUserRoles (qué rol tiene cada usuario),
/// AspNetUserTokens (tokens, como el de recuperar contraseña), etc.
/// </summary>
public class AppDbContext : IdentityDbContext<Usuario>
{
    /// <summary>
    /// Constructor. Recibe las opciones de configuración (cadena de
    /// conexión y proveedor SQL Server) desde Program.cs mediante
    /// inyección de dependencias, y se las pasa a la clase base.
    /// </summary>
    public AppDbContext(DbContextOptions<AppDbContext> opciones) : base(opciones)
    {
    }

    /// <summary>
    /// Aquí se ajustan las reglas del modelo de datos (tamaños,
    /// campos obligatorios, índices). Se ejecuta cuando EF Core
    /// construye el modelo y al generar las migraciones.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // IMPORTANTE: primero se llama a la base para que Identity
        // configure sus propias tablas. Si se omite, fallan las migraciones.
        base.OnModelCreating(modelBuilder);

        // Regla propia: el nombre completo es obligatorio (NOT NULL
        // en SQL Server) y admite como máximo 100 caracteres.
        modelBuilder.Entity<Usuario>()
            .Property(u => u.NombreCompleto)
            .IsRequired()
            .HasMaxLength(100);
    }
}