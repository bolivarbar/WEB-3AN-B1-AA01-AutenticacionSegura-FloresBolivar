using AutenticacionSegura.Negocio.Modelos;

namespace AutenticacionSegura.Negocio.Servicios;

/// <summary>
/// Contrato del flujo "Olvidé mi contraseña":
/// 1) solicitar el enlace por correo y 2) restablecer la contraseña con el token.
/// </summary>
public interface IServicioRecuperacion
{
    /// <summary>
    /// Genera un token de restablecimiento y envía un correo con el enlace.
    /// Responde igual exista o no el correo, para no revelar qué cuentas existen.
    /// </summary>
    /// <param name="correo">Correo ingresado por la persona.</param>
    /// <param name="construirEnlace">
    /// Función que recibe (correo, token) y devuelve la URL completa del enlace.
    /// La arma la capa Web, que es la que conoce las rutas de la aplicación.
    /// </param>
    Task<ResultadoOperacion> SolicitarRestablecimientoAsync(
        string correo, Func<string, string, string> construirEnlace);

    /// <summary>
    /// Cambia la contraseña usando el token recibido por correo.
    /// Falla si el token es inválido, venció o ya se usó.
    /// </summary>
    Task<ResultadoOperacion> RestablecerClaveAsync(
        string correo, string token, string nuevaClave);
}