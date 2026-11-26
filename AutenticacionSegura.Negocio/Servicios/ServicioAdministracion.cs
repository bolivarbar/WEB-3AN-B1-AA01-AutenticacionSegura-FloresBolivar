using AutenticacionSegura.Datos.Entidades;
using AutenticacionSegura.Negocio.Modelos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AutenticacionSegura.Negocio.Servicios;

/// <summary>
/// Administración de usuarios sobre ASP.NET Core Identity.
/// Reglas que aplica este servicio (independientes del controlador):
///  1. Solo un Administrador ACTIVO puede ejecutar estas operaciones.
///  2. Solo existen los roles Cliente y Administrador.
///  3. Nunca puede quedar el sistema sin un administrador activo: no se puede
///     quitar el rol ni deshabilitar al último administrador activo.
///  4. Al cambiar el rol o deshabilitar una cuenta se renueva su SecurityStamp,
///     lo que invalida las cookies que ese usuario ya tenía emitidas.
/// </summary>
public class ServicioAdministracion : IServicioAdministracion
{
    private const string MensajeSinPermiso = "No tiene permisos para realizar esta operación.";
    private const string MensajeUsuarioNoEncontrado = "No se encontró el usuario.";

    private readonly UserManager<Usuario> _userManager;

    public ServicioAdministracion(UserManager<Usuario> userManager)
    {
        _userManager = userManager;
    }

    // ---------------------------------------------------------------
    // LISTADO
    // ---------------------------------------------------------------

    /// <inheritdoc />
    public async Task<IReadOnlyList<UsuarioResumen>> ListarUsuariosAsync()
    {
        var usuarios = await _userManager.Users.OrderBy(u => u.Email).ToListAsync();
        var resultado = new List<UsuarioResumen>();

        foreach (var usuario in usuarios)
        {
            // Cada usuario tiene un único rol en este sistema; si no tuviera ninguno
            // se muestra como Cliente (rol por defecto).
            var roles = await _userManager.GetRolesAsync(usuario);

            resultado.Add(new UsuarioResumen
            {
                Id = usuario.Id,
                Correo = usuario.Email ?? string.Empty,
                NombreCompleto = usuario.NombreCompleto,
                Rol = roles.FirstOrDefault() ?? RolesSistema.Cliente,
                Activo = usuario.Activo,
                FechaRegistro = usuario.FechaRegistro
            });
        }

        return resultado;
    }

    // ---------------------------------------------------------------
    // CAMBIO DE ROL
    // ---------------------------------------------------------------

    /// <inheritdoc />
    public async Task<ResultadoOperacion> CambiarRolAsync(string idActor, string idUsuario, string nuevoRol)
    {
        // Regla 1: quien ejecuta debe ser Administrador activo (prueba 10 del PDF:
        // un Cliente que intente asignar Administrador es rechazado aquí).
        if (!await EsAdministradorActivoAsync(idActor))
            return ResultadoOperacion.Fallo(MensajeSinPermiso);

        // Regla 2: solo se aceptan los roles definidos en el sistema.
        if (nuevoRol != RolesSistema.Cliente && nuevoRol != RolesSistema.Administrador)
            return ResultadoOperacion.Fallo("El rol indicado no es válido.");

        var usuario = await _userManager.FindByIdAsync(idUsuario);
        if (usuario is null)
            return ResultadoOperacion.Fallo(MensajeUsuarioNoEncontrado);

        var rolesActuales = await _userManager.GetRolesAsync(usuario);

        // Si ya tiene exactamente ese rol, no hay nada que hacer.
        if (rolesActuales.Count == 1 && rolesActuales[0] == nuevoRol)
            return ResultadoOperacion.Ok();

        // Regla 3: no se le puede quitar el rol de Administrador al último administrador activo.
        var esAdministradorActivo = usuario.Activo && rolesActuales.Contains(RolesSistema.Administrador);
        if (esAdministradorActivo && nuevoRol != RolesSistema.Administrador
            && await ContarAdministradoresActivosAsync() <= 1)
        {
            return ResultadoOperacion.Fallo("No se puede quitar el rol al último administrador activo.");
        }

        // Se quitan los roles que ya no corresponden y se agrega el nuevo.
        var rolesQuitar = rolesActuales.Where(r => r != nuevoRol).ToList();
        if (rolesQuitar.Count > 0)
        {
            var quitar = await _userManager.RemoveFromRolesAsync(usuario, rolesQuitar);
            if (!quitar.Succeeded) return Fallo(quitar);
        }

        if (!rolesActuales.Contains(nuevoRol))
        {
            var agregar = await _userManager.AddToRoleAsync(usuario, nuevoRol);
            if (!agregar.Succeeded) return Fallo(agregar);
        }

        // Regla 4: el rol viaja dentro de la cookie; renovar el SecurityStamp obliga
        // a que la cookie anterior deje de ser válida y el usuario reciba una nueva.
        await _userManager.UpdateSecurityStampAsync(usuario);

        return ResultadoOperacion.Ok();
    }

    // ---------------------------------------------------------------
    // HABILITAR / DESHABILITAR
    // ---------------------------------------------------------------

    /// <inheritdoc />
    public async Task<ResultadoOperacion> CambiarEstadoAsync(string idActor, string idUsuario, bool activo)
    {
        // Regla 1: solo un Administrador activo puede cambiar el estado de cuentas.
        if (!await EsAdministradorActivoAsync(idActor))
            return ResultadoOperacion.Fallo(MensajeSinPermiso);

        var usuario = await _userManager.FindByIdAsync(idUsuario);
        if (usuario is null)
            return ResultadoOperacion.Fallo(MensajeUsuarioNoEncontrado);

        // Si ya está en ese estado, no hay nada que hacer.
        if (usuario.Activo == activo)
            return ResultadoOperacion.Ok();

        // Regla 3: no se puede deshabilitar al último administrador activo.
        if (!activo
            && await _userManager.IsInRoleAsync(usuario, RolesSistema.Administrador)
            && await ContarAdministradoresActivosAsync() <= 1)
        {
            return ResultadoOperacion.Fallo("No se puede deshabilitar al último administrador activo.");
        }

        usuario.Activo = activo;
        var actualizar = await _userManager.UpdateAsync(usuario);
        if (!actualizar.Succeeded) return Fallo(actualizar);

        // Regla 4: al deshabilitar, se invalidan las sesiones que ya tuviera abiertas.
        if (!activo)
            await _userManager.UpdateSecurityStampAsync(usuario);

        return ResultadoOperacion.Ok();
    }

    // ---------------------------------------------------------------
    // AUXILIARES PRIVADOS
    // ---------------------------------------------------------------

    /// <summary>
    /// true si el id corresponde a un usuario ACTIVO que tiene el rol Administrador.
    /// La comprobación se hace contra la base de datos, no contra lo que diga la cookie.
    /// </summary>
    private async Task<bool> EsAdministradorActivoAsync(string idActor)
    {
        if (string.IsNullOrWhiteSpace(idActor)) return false;

        var actor = await _userManager.FindByIdAsync(idActor);
        return actor is not null
            && actor.Activo
            && await _userManager.IsInRoleAsync(actor, RolesSistema.Administrador);
    }

    /// <summary>Cuenta cuántos administradores tienen la cuenta habilitada.</summary>
    private async Task<int> ContarAdministradoresActivosAsync()
    {
        var administradores = await _userManager.GetUsersInRoleAsync(RolesSistema.Administrador);
        return administradores.Count(a => a.Activo);
    }

    /// <summary>Convierte un IdentityResult con errores en un ResultadoOperacion.Fallo.</summary>
    private static ResultadoOperacion Fallo(IdentityResult resultado) =>
        ResultadoOperacion.Fallo(string.Join(" ", resultado.Errors.Select(e => e.Description)));
}