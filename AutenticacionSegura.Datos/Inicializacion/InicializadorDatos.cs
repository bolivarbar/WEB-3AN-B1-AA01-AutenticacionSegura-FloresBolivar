using AutenticacionSegura.Datos.Entidades;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AutenticacionSegura.Datos.Inicializacion;

/// <summary>
/// Prepara los datos mínimos que el sistema necesita para funcionar:
///  1. Crea los roles "Cliente" y "Administrador" si no existen.
///  2. Crea el administrador inicial si no existe (seeding controlado).
/// Es seguro ejecutarlo en cada arranque: si ya existen, no duplica nada.
/// </summary>
public static class InicializadorDatos
{
    /// <param name="servicios">Contenedor de servicios de la aplicación (se usa para pedir RoleManager y UserManager).</param>
    /// <param name="correoAdmin">Correo del administrador inicial. Viene de User Secrets, no del código.</param>
    /// <param name="claveAdmin">Contraseña del administrador inicial. Viene de User Secrets, no del código.</param>
    public static async Task EjecutarAsync(IServiceProvider servicios, string? correoAdmin, string? claveAdmin)
    {
        // Se crea un "alcance" porque UserManager y RoleManager viven por petición,
        // y aquí no hay una petición web: estamos en el arranque.
        using var alcance = servicios.CreateScope();
        var gestorRoles = alcance.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var gestorUsuarios = alcance.ServiceProvider.GetRequiredService<UserManager<Usuario>>();

        // 1. Roles: se crean solo si todavía no existen.
        string[] roles = { RolesSistema.Cliente, RolesSistema.Administrador };
        foreach (var rol in roles)
        {
            if (!await gestorRoles.RoleExistsAsync(rol))
            {
                await gestorRoles.CreateAsync(new IdentityRole(rol));
            }
        }

        // 2. Administrador inicial: si faltan los secretos, no se crea nadie.
        // Así nunca existe un administrador con una contraseña por defecto.
        if (string.IsNullOrWhiteSpace(correoAdmin) || string.IsNullOrWhiteSpace(claveAdmin))
        {
            return;
        }

        var admin = await gestorUsuarios.FindByEmailAsync(correoAdmin);
        if (admin is null)
        {
            admin = new Usuario
            {
                UserName = correoAdmin,           // El login será con el correo
                Email = correoAdmin,
                EmailConfirmed = true,            // Cuenta de confianza creada por el sistema
                NombreCompleto = "Administrador inicial",
                Activo = true
            };

            // CreateAsync guarda la contraseña con hash (PasswordHasher de Identity),
            // nunca en texto legible.
            var resultado = await gestorUsuarios.CreateAsync(admin, claveAdmin);
            if (!resultado.Succeeded)
            {
                var errores = string.Join("; ", resultado.Errors.Select(e => e.Description));
                throw new InvalidOperationException("No se pudo crear el administrador inicial: " + errores);
            }
        }

        // Garantiza que tenga el rol de Administrador (aunque ya existiera el usuario).
        if (!await gestorUsuarios.IsInRoleAsync(admin, RolesSistema.Administrador))
        {
            await gestorUsuarios.AddToRoleAsync(admin, RolesSistema.Administrador);
        }
    }
}