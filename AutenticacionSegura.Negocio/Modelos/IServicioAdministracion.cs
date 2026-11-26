using AutenticacionSegura.Negocio.Modelos;

namespace AutenticacionSegura.Negocio.Servicios;

/// <summary>
/// Contrato de la administración de usuarios (solo para el rol Administrador).
/// Cada operación recibe el id del administrador que la ejecuta ("idActor") para
/// que el servicio vuelva a comprobar que realmente tiene ese rol: la seguridad
/// no depende únicamente del [Authorize] del controlador.
/// </summary>
public interface IServicioAdministracion
{
    /// <summary>Devuelve todos los usuarios con su rol y estado, ordenados por correo.</summary>
    Task<IReadOnlyList<UsuarioResumen>> ListarUsuariosAsync();

    /// <summary>
    /// Cambia el rol de un usuario ("Cliente" o "Administrador").
    /// Rechaza la operación si quien la ejecuta no es Administrador, si el rol no
    /// existe, o si se intenta quitar el rol al último administrador activo.
    /// </summary>
    Task<ResultadoOperacion> CambiarRolAsync(string idActor, string idUsuario, string nuevoRol);

    /// <summary>
    /// Habilita o deshabilita una cuenta.
    /// Rechaza la operación si quien la ejecuta no es Administrador o si se intenta
    /// deshabilitar al último administrador activo.
    /// </summary>
    Task<ResultadoOperacion> CambiarEstadoAsync(string idActor, string idUsuario, bool activo);
}