using AutenticacionSegura.Datos.Entidades;
using AutenticacionSegura.Negocio.Modelos;

namespace AutenticacionSegura.Negocio.Servicios;

/// <summary>
/// Contrato de la capa de Negocio para el perfil del usuario autenticado:
/// consultar sus datos, editar su nombre y cambiar su contraseña.
/// La capa Web solo conoce esta interfaz, nunca UserManager directamente.
/// </summary>
public interface IServicioPerfil
{
    /// <summary>
    /// Obtiene los datos del usuario a partir de su identificador.
    /// Devuelve null si el usuario no existe.
    /// </summary>
    Task<Usuario?> ObtenerPerfilAsync(string idUsuario);

    /// <summary>
    /// Actualiza el nombre completo del usuario.
    /// </summary>
    Task<ResultadoOperacion> ActualizarNombreAsync(string idUsuario, string nombreCompleto);

    /// <summary>
    /// Cambia la contraseña del usuario. Exige la contraseña actual correcta:
    /// si es incorrecta, la contraseña NO se modifica (prueba 11 del PDF).
    /// </summary>
    Task<ResultadoOperacion> CambiarClaveAsync(string idUsuario, string claveActual, string claveNueva);
}