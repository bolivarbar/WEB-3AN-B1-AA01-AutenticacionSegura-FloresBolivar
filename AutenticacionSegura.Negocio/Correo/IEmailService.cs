using AutenticacionSegura.Negocio.Modelos;

namespace AutenticacionSegura.Negocio.Correo;

/// <summary>
/// Contrato para el envío de correos electrónicos.
///
/// Los servicios de la capa de Negocio (por ejemplo, la recuperación de
/// contraseña) dependen de esta interfaz y no de una tecnología concreta.
/// Así, el código que "pide enviar un correo" no sabe si se usa SMTP de Gmail,
/// otro servidor o una API de proveedor: solo conoce este contrato.
/// La implementación real por SMTP se crea en el paso 8.5.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Envía un correo HTML a un destinatario.
    /// </summary>
    /// <param name="destinatario">Dirección de correo de destino.</param>
    /// <param name="asunto">Asunto del mensaje.</param>
    /// <param name="cuerpoHtml">Contenido del mensaje en formato HTML.</param>
    /// <returns>
    /// Un <see cref="ResultadoOperacion"/>: éxito si el servidor aceptó el
    /// mensaje, o fallo con el motivo (sin lanzar excepciones al llamador).
    /// </returns>
    Task<ResultadoOperacion> EnviarAsync(string destinatario, string asunto, string cuerpoHtml);
}