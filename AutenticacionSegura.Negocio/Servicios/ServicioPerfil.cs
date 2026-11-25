using AutenticacionSegura.Datos.Entidades;
using AutenticacionSegura.Negocio.Modelos;
using Microsoft.AspNetCore.Identity;

namespace AutenticacionSegura.Negocio.Servicios;

/// <summary>
/// Implementación del perfil del usuario sobre ASP.NET Core Identity.
/// </summary>
public class ServicioPerfil : IServicioPerfil
{
    // Mensaje fijo cuando la contraseña actual no coincide.
    private const string MensajeClaveActualIncorrecta = "La contraseña actual no es correcta.";

    private readonly UserManager<Usuario> _userManager;
    private readonly SignInManager<Usuario> _signInManager;

    public ServicioPerfil(UserManager<Usuario> userManager, SignInManager<Usuario> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    /// <inheritdoc />
    public async Task<Usuario?> ObtenerPerfilAsync(string idUsuario)
    {
        if (string.IsNullOrWhiteSpace(idUsuario)) return null;
        return await _userManager.FindByIdAsync(idUsuario);
    }

    /// <inheritdoc />
    public async Task<ResultadoOperacion> ActualizarNombreAsync(string idUsuario, string nombreCompleto)
    {
        if (string.IsNullOrWhiteSpace(nombreCompleto))
            return ResultadoOperacion.Fallo("El nombre no puede estar vacío.");

        var usuario = await ObtenerPerfilAsync(idUsuario);
        if (usuario is null)
            return ResultadoOperacion.Fallo("No se encontró el usuario.");

        usuario.NombreCompleto = nombreCompleto.Trim();
        var resultado = await _userManager.UpdateAsync(usuario);

        return resultado.Succeeded
            ? ResultadoOperacion.Ok()
            : ResultadoOperacion.Fallo(string.Join(" ", resultado.Errors.Select(e => e.Description)));
    }

    /// <inheritdoc />
    public async Task<ResultadoOperacion> CambiarClaveAsync(string idUsuario, string claveActual, string claveNueva)
    {
        var usuario = await ObtenerPerfilAsync(idUsuario);
        if (usuario is null)
            return ResultadoOperacion.Fallo("No se encontró el usuario.");

        // ChangePasswordAsync verifica primero la contraseña actual.
        // Si no coincide, devuelve PasswordMismatch y NO modifica nada.
        var resultado = await _userManager.ChangePasswordAsync(usuario, claveActual, claveNueva);

        if (!resultado.Succeeded)
        {
            if (resultado.Errors.Any(e => e.Code == nameof(IdentityErrorDescriber.PasswordMismatch)))
                return ResultadoOperacion.Fallo(MensajeClaveActualIncorrecta);

            return ResultadoOperacion.Fallo(string.Join(" ", resultado.Errors.Select(e => e.Description)));
        }

        // Cambiar la contraseña renueva el SecurityStamp, lo que invalidaría la cookie
        // actual. Se vuelve a emitir para que el usuario siga con la sesión abierta.
        await _signInManager.RefreshSignInAsync(usuario);

        return ResultadoOperacion.Ok();
    }
}