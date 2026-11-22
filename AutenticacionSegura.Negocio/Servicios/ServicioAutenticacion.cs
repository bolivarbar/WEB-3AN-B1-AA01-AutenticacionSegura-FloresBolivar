using AutenticacionSegura.Datos.Entidades;
using AutenticacionSegura.Negocio.Modelos;
using Microsoft.AspNetCore.Identity;

namespace AutenticacionSegura.Negocio.Servicios;

/// <summary>
/// Implementación de las reglas de negocio de autenticación.
/// Usa UserManager (ASP.NET Core Identity) para crear usuarios y asignar roles;
/// no se implementa ningún algoritmo criptográfico propio.
/// </summary>
public class ServicioAutenticacion : IServicioAutenticacion
{
    private readonly UserManager<Usuario> _gestorUsuarios;

    // El UserManager llega por inyección de dependencias (lo registra AddIdentity).
    public ServicioAutenticacion(UserManager<Usuario> gestorUsuarios)
    {
        _gestorUsuarios = gestorUsuarios;
    }

    public async Task<ResultadoOperacion> RegistrarAsync(string nombreCompleto, string correo, string clave)
    {
        // 1. Reglas propias: campos obligatorios (validación del lado del servidor).
        if (string.IsNullOrWhiteSpace(nombreCompleto))
            return ResultadoOperacion.Fallo("El nombre completo es obligatorio.");

        if (string.IsNullOrWhiteSpace(correo))
            return ResultadoOperacion.Fallo("El correo es obligatorio.");

        if (string.IsNullOrWhiteSpace(clave))
            return ResultadoOperacion.Fallo("La contraseña es obligatoria.");

        correo = correo.Trim();

        // 2. Correo único: si ya existe, se rechaza el registro.
        if (await _gestorUsuarios.FindByEmailAsync(correo) is not null)
            return ResultadoOperacion.Fallo("Ese correo ya está registrado.");

        // 3. Se arma el usuario. El correo también sirve de nombre de usuario.
        var usuario = new Usuario
        {
            UserName = correo,
            Email = correo,
            NombreCompleto = nombreCompleto.Trim(),
            Activo = true
        };

        // 4. CreateAsync valida la política de contraseña, comprueba duplicados
        // y guarda la contraseña con hash (PasswordHasher de Identity).
        var resultado = await _gestorUsuarios.CreateAsync(usuario, clave);
        if (!resultado.Succeeded)
            return ResultadoOperacion.Fallo(resultado.Errors.Select(e => e.Description));

        // 5. Todo usuario nuevo recibe SIEMPRE el rol Cliente.
        var rolAsignado = await _gestorUsuarios.AddToRoleAsync(usuario, RolesSistema.Cliente);
        if (!rolAsignado.Succeeded)
        {
            // Si no se pudo asignar el rol, se elimina el usuario para no dejar
            // una cuenta sin rol a medias.
            await _gestorUsuarios.DeleteAsync(usuario);
            return ResultadoOperacion.Fallo("No se pudo completar el registro. Intente nuevamente.");
        }

        return ResultadoOperacion.Ok();
    }
}