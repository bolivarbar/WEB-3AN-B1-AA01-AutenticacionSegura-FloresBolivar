using AutenticacionSegura.Negocio.Servicios;
using AutenticacionSegura.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace AutenticacionSegura.Web.Controllers;

/// <summary>
/// Controlador del flujo "Olvidé mi contraseña".
///
/// Pantallas:
///  1) OlvideClave               -> la persona escribe su correo.
///  2) OlvideClaveConfirmacion   -> aviso "revise su correo".
///  3) RestablecerClave          -> formulario de nueva contraseña (llega desde el enlace).
///
/// Toda la lógica (token, correo, reglas) está en IServicioRecuperacion (Negocio);
/// este controlador solo recibe formularios, llama al servicio y elige la vista.
/// </summary>
public class RecuperacionController : Controller
{
    private readonly IServicioRecuperacion _recuperacion;

    public RecuperacionController(IServicioRecuperacion recuperacion)
    {
        _recuperacion = recuperacion;
    }

    // ------------------------------------------------------------------
    // 1) Formulario "Olvidé mi contraseña": GET (mostrar) y POST (enviar).
    // ------------------------------------------------------------------

    /// <summary>Muestra el formulario donde se escribe el correo.</summary>
    [HttpGet]
    public IActionResult OlvideClave()
    {
        return View(new OlvideClaveViewModel());
    }

    /// <summary>
    /// Recibe el correo y pide al servicio que envíe el enlace de recuperación.
    /// Siempre muestra la misma confirmación, exista o no el correo.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken] // protege el formulario contra CSRF
    public async Task<IActionResult> OlvideClave(OlvideClaveViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        // Función que arma la URL absoluta del enlace del correo:
        // /Recuperacion/RestablecerClave?correo=...&token=...
        // Url.Action codifica el token para que viaje seguro en la URL.
        string ConstruirEnlace(string correo, string token) =>
            Url.Action(nameof(RestablecerClave), "Recuperacion",
                new { correo, token }, Request.Scheme)!;

        var resultado = await _recuperacion.SolicitarRestablecimientoAsync(
            modelo.Correo, ConstruirEnlace);

        if (!resultado.Exitoso)
        {
            foreach (var error in resultado.Errores)
            {
                ModelState.AddModelError(string.Empty, error);
            }
            return View(modelo);
        }

        return RedirectToAction(nameof(OlvideClaveConfirmacion));
    }

    // ------------------------------------------------------------------
    // 2) Aviso de "revise su correo".
    // ------------------------------------------------------------------

    /// <summary>Muestra el mensaje genérico de que se envió (si aplica) el enlace.</summary>
    [HttpGet]
    public IActionResult OlvideClaveConfirmacion()
    {
        return View();
    }

    // ------------------------------------------------------------------
    // 3) Nueva contraseña: GET (desde el enlace del correo) y POST (guardar).
    // ------------------------------------------------------------------

    /// <summary>
    /// Muestra el formulario de nueva contraseña. Correo y token vienen en el
    /// enlace del correo y se guardan en campos ocultos del formulario.
    /// </summary>
    [HttpGet]
    public IActionResult RestablecerClave(string? correo, string? token)
    {
        if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(token))
        {
            // Enlace incompleto o manipulado: no hay nada que restablecer.
            return View("EnlaceInvalido");
        }

        return View(new RestablecerClaveViewModel { Correo = correo, Token = token });
    }

    /// <summary>
    /// Cambia la contraseña. El servicio valida el token (vigencia y un solo uso)
    /// y la política de contraseñas.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestablecerClave(RestablecerClaveViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var resultado = await _recuperacion.RestablecerClaveAsync(
            modelo.Correo, modelo.Token, modelo.Clave);

        if (!resultado.Exitoso)
        {
            // Token inválido, vencido, reutilizado o contraseña que no cumple la política.
            foreach (var error in resultado.Errores)
            {
                ModelState.AddModelError(string.Empty, error);
            }
            return View(modelo);
        }

        // Éxito: se avisa en la pantalla de login (el layout muestra TempData["Mensaje"]).
        TempData["Mensaje"] = "Su contraseña se cambió correctamente. Ya puede iniciar sesión.";
        return RedirectToAction("Login", "Cuenta");
    }

    /// <summary>Pantalla para enlaces incompletos o inválidos.</summary>
    [HttpGet]
    public IActionResult EnlaceInvalido()
    {
        return View();
    }
}