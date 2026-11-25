using System.Net;
using AutenticacionSegura.Datos.Entidades;
using AutenticacionSegura.Negocio.Correo;
using AutenticacionSegura.Negocio.Modelos;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AutenticacionSegura.Negocio.Servicios;

/// <summary>
/// Implementa la recuperación de contraseña con los tokens de ASP.NET Core Identity.
/// Los tokens vencen según Autenticacion:VigenciaTokenRecuperacionMinutos y son
/// de un solo uso: al cambiar la contraseña cambia el "sello de seguridad" del
/// usuario y cualquier token anterior queda inválido.
/// </summary>
public class ServicioRecuperacion : IServicioRecuperacion
{
    // Mensaje único para token inválido, vencido, reutilizado o usuario inexistente.
    private const string MensajeEnlaceInvalido =
        "El enlace no es válido o ya venció. Solicite uno nuevo.";

    private readonly UserManager<Usuario> _userManager;
    private readonly IEmailService _correo;
    private readonly DataProtectionTokenProviderOptions _opcionesToken;
    private readonly ILogger<ServicioRecuperacion> _logger;

    public ServicioRecuperacion(
        UserManager<Usuario> userManager,
        IEmailService correo,
        IOptions<DataProtectionTokenProviderOptions> opcionesToken,
        ILogger<ServicioRecuperacion> logger)
    {
        _userManager = userManager;
        _correo = correo;
        _opcionesToken = opcionesToken.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ResultadoOperacion> SolicitarRestablecimientoAsync(
        string correo, Func<string, string, string> construirEnlace)
    {
        if (string.IsNullOrWhiteSpace(correo))
        {
            return ResultadoOperacion.Fallo("Ingrese su correo electrónico.");
        }

        var usuario = await _userManager.FindByEmailAsync(correo.Trim());

        // Si el correo no existe o la cuenta está deshabilitada, NO se envía nada
        // pero se responde igual que en el caso exitoso (no se revela si existe).
        if (usuario is null || !usuario.Activo)
        {
            return ResultadoOperacion.Ok();
        }

        // Token generado por Identity; vence según la vigencia configurada.
        var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
        var enlace = construirEnlace(usuario.Email!, token);
        var minutos = (int)_opcionesToken.TokenLifespan.TotalMinutes;

        // Se codifican los textos que van dentro del HTML del correo.
        var nombre = WebUtility.HtmlEncode(usuario.NombreCompleto);
        var enlaceHtml = WebUtility.HtmlEncode(enlace);

        var cuerpo = $"""
            <p>Hola {nombre},</p>
            <p>Recibimos una solicitud para restablecer su contraseña.
            Haga clic en el siguiente enlace para crear una nueva:</p>
            <p><a href="{enlaceHtml}">Restablecer mi contraseña</a></p>
            <p>El enlace vence en {minutos} minutos y solo puede usarse una vez.
            Si usted no hizo esta solicitud, ignore este mensaje.</p>
            """;

        var resultado = await _correo.EnviarAsync(
            usuario.Email!, "Recuperación de contraseña", cuerpo);

        // Si el envío falla solo se registra en el log; al usuario se le responde
        // igual para no revelar si la cuenta existe.
        if (!resultado.Exitoso)
        {
            _logger.LogWarning("No se pudo enviar el correo de recuperación: {Motivo}",
                string.Join(" ", resultado.Errores));
        }

        return ResultadoOperacion.Ok();
    }

    /// <inheritdoc />
    public async Task<ResultadoOperacion> RestablecerClaveAsync(
        string correo, string token, string nuevaClave)
    {
        if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(token))
        {
            return ResultadoOperacion.Fallo(MensajeEnlaceInvalido);
        }

        var usuario = await _userManager.FindByEmailAsync(correo.Trim());
        if (usuario is null || !usuario.Activo)
        {
            return ResultadoOperacion.Fallo(MensajeEnlaceInvalido);
        }

        // Identity valida el token (vigencia y sello de seguridad) y aplica la
        // política de contraseñas. Al terminar bien, el token queda inutilizado.
        var resultado = await _userManager.ResetPasswordAsync(usuario, token, nuevaClave);

        if (resultado.Succeeded)
        {
            return ResultadoOperacion.Ok();
        }

        // Token inválido/vencido/reutilizado: mensaje único.
        if (resultado.Errors.Any(e => e.Code == nameof(IdentityErrorDescriber.InvalidToken)))
        {
            return ResultadoOperacion.Fallo(MensajeEnlaceInvalido);
        }

        // Otros errores (por ejemplo, la contraseña no cumple la política).
        return ResultadoOperacion.Fallo(resultado.Errors.Select(e => e.Description));
    }
}