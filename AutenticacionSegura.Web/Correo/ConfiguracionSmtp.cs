namespace AutenticacionSegura.Negocio.Correo;

/// <summary>
/// Datos de configuración del servidor SMTP.
///
/// Se llena automáticamente con la sección "Smtp" de la configuración:
///  - Servidor, Puerto, UsarStartTls y NombreRemitente vienen de
///    appsettings.json (no son secretos y sí se suben a GitHub).
///  - Usuario y Clave vienen de User Secrets (secretos: NUNCA se suben).
/// .NET une ambos orígenes bajo el mismo nombre de sección.
/// </summary>
public class ConfiguracionSmtp
{
    /// <summary>Nombre de la sección en la configuración.</summary>
    public const string NombreSeccion = "Smtp";

    /// <summary>Servidor SMTP (para Gmail: smtp.gmail.com).</summary>
    public string Servidor { get; set; } = string.Empty;

    /// <summary>Puerto del servidor (587 usa cifrado STARTTLS).</summary>
    public int Puerto { get; set; } = 587;

    /// <summary>Si es true, la conexión se cifra con STARTTLS.</summary>
    public bool UsarStartTls { get; set; } = true;

    /// <summary>Nombre que verá el destinatario como remitente.</summary>
    public string NombreRemitente { get; set; } = string.Empty;

    /// <summary>Cuenta que envía el correo (viene de User Secrets).</summary>
    public string Usuario { get; set; } = string.Empty;

    /// <summary>
    /// Contraseña de aplicación (viene de User Secrets).
    /// Nunca debe escribirse en logs, mensajes de error ni en el repositorio.
    /// </summary>
    public string Clave { get; set; } = string.Empty;
}