using AutenticacionSegura.Negocio.Modelos;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AutenticacionSegura.Negocio.Correo;

/// <summary>
/// Implementación de <see cref="IEmailService"/> que envía correos reales
/// mediante un servidor SMTP (en este proyecto, el de Gmail) usando MailKit.
///
/// Los datos de conexión llegan en <see cref="ConfiguracionSmtp"/>, que se
/// llena desde appsettings.json (datos públicos) y User Secrets (usuario y clave).
/// </summary>
public class ServicioCorreoSmtp : IEmailService
{
    private readonly ConfiguracionSmtp _config;
    private readonly ILogger<ServicioCorreoSmtp> _logger;

    /// <summary>
    /// Recibe la configuración SMTP y un registrador de eventos (logger)
    /// mediante inyección de dependencias.
    /// </summary>
    public ServicioCorreoSmtp(IOptions<ConfiguracionSmtp> opciones, ILogger<ServicioCorreoSmtp> logger)
    {
        _config = opciones.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ResultadoOperacion> EnviarAsync(string destinatario, string asunto, string cuerpoHtml)
    {
        // 1) Verificar que haya configuración mínima. Si falta el usuario o la
        //    clave (por ejemplo, no se configuraron los User Secrets), no se
        //    intenta conectar y se informa de forma controlada.
        if (string.IsNullOrWhiteSpace(_config.Servidor) ||
            string.IsNullOrWhiteSpace(_config.Usuario) ||
            string.IsNullOrWhiteSpace(_config.Clave))
        {
            _logger.LogError("El servicio de correo no está configurado (faltan Servidor, Usuario o Clave).");
            return ResultadoOperacion.Fallo("El servicio de correo no está configurado.");
        }

        if (string.IsNullOrWhiteSpace(destinatario))
        {
            return ResultadoOperacion.Fallo("Falta la dirección de correo del destinatario.");
        }

        try
        {
            // 2) Armar el mensaje: remitente, destinatario, asunto y cuerpo HTML.
            var mensaje = new MimeMessage();
            mensaje.From.Add(new MailboxAddress(_config.NombreRemitente, _config.Usuario));
            mensaje.To.Add(MailboxAddress.Parse(destinatario)); // lanza FormatException si es inválido
            mensaje.Subject = asunto;
            mensaje.Body = new BodyBuilder { HtmlBody = cuerpoHtml }.ToMessageBody();

            // 3) Conectar al servidor. Con STARTTLS la conexión empieza normal
            //    y se cifra antes de enviar la clave.
            using var cliente = new SmtpClient();
            var seguridad = _config.UsarStartTls
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.Auto;

            await cliente.ConnectAsync(_config.Servidor, _config.Puerto, seguridad);

            // 4) Autenticarse con la cuenta y la contraseña de aplicación.
            await cliente.AuthenticateAsync(_config.Usuario, _config.Clave);

            // 5) Enviar y cerrar la conexión de forma ordenada.
            await cliente.SendAsync(mensaje);
            await cliente.DisconnectAsync(true);

            return ResultadoOperacion.Ok();
        }
        catch (FormatException)
        {
            // La dirección del destinatario no tiene formato de correo.
            return ResultadoOperacion.Fallo("La dirección de correo no es válida.");
        }
        catch (AuthenticationException ex)
        {
            // Usuario o contraseña de aplicación rechazados por el servidor.
            _logger.LogError(ex, "El servidor SMTP rechazó las credenciales.");
            return ResultadoOperacion.Fallo("No se pudo autenticar con el servidor de correo.");
        }
        catch (Exception ex)
        {
            // Cualquier otro fallo (red, servidor caído, etc.). Se registra el
            // detalle técnico, pero al usuario solo se le da un mensaje genérico.
            // Nunca se registran la clave ni la dirección del destinatario.
            _logger.LogError(ex, "Error al enviar el correo por SMTP.");
            return ResultadoOperacion.Fallo("No se pudo enviar el correo. Intente de nuevo más tarde.");
        }
    }
}