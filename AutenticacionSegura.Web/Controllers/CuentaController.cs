using AutenticacionSegura.Negocio.Servicios;
using AutenticacionSegura.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutenticacionSegura.Web.Controllers;

/// <summary>
/// Controlador de cuentas (capa de Presentación).
/// Solo COORDINA: recibe el formulario, llama al servicio de Negocio y decide
/// qué vista mostrar. No contiene reglas de negocio ni consultas a la base de
/// datos; eso es responsabilidad de las otras capas (arquitectura de 3 capas).
/// </summary>
public class CuentaController : Controller
{
    private readonly IServicioAutenticacion _servicio;

    // El servicio llega por inyección de dependencias (se registró en Program.cs).
    // El controlador depende de la INTERFAZ, no de la implementación concreta.
    public CuentaController(IServicioAutenticacion servicio)
    {
        _servicio = servicio;
    }

    // ==================== REGISTRO ====================

    /// <summary>GET /Cuenta/Registro: muestra el formulario de registro vacío.</summary>
    [HttpGet]
    public IActionResult Registro()
    {
        return View();
    }

    /// <summary>
    /// POST /Cuenta/Registro: procesa el formulario.
    /// [ValidateAntiForgeryToken] rechaza envíos que no traigan el token
    /// antiforgery válido (protección contra ataques CSRF).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registro(RegistroViewModel modelo)
    {
        // 1. Validaciones del modelo en el SERVIDOR (obligatorios, formato, confirmación).
        //    Nunca se confía solo en la validación del navegador, porque puede saltarse.
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        // 2. La lógica real (correo único, política de contraseña, rol Cliente)
        //    la hace la capa de Negocio.
        var resultado = await _servicio.RegistrarAsync(modelo.NombreCompleto, modelo.Correo, modelo.Clave);

        // 3. Si falló, se muestran los errores en el formulario.
        if (!resultado.Exitoso)
        {
            foreach (var error in resultado.Errores)
            {
                // string.Empty = error general (no ligado a un campo concreto).
                ModelState.AddModelError(string.Empty, error);
            }
            return View(modelo);
        }

        // 4. Éxito: mensaje temporal (TempData sobrevive a una redirección)
        //    y se envía al usuario a iniciar sesión.
        TempData["Mensaje"] = "Cuenta creada correctamente. Ya puede iniciar sesión.";
        return RedirectToAction(nameof(Login));
    }

    // ==================== INICIO DE SESIÓN ====================

    /// <summary>
    /// GET /Cuenta/Login: muestra el formulario de inicio de sesión.
    /// El parámetro se llama "returnUrl" porque así lo envía el sistema de cookies
    /// cuando alguien intenta entrar a una página protegida sin haber iniciado sesión.
    /// </summary>
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        // Si ya hay sesión iniciada, no tiene sentido mostrar el login.
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(nameof(AreaPersonal));
        }

        return View(new LoginViewModel { UrlRetorno = returnUrl });
    }

    /// <summary>POST /Cuenta/Login: valida las credenciales y crea la sesión.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel modelo)
    {
        // 1. Validación del modelo en el servidor (campos vacíos, formato de correo).
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        // 2. El servicio de Negocio verifica el hash de la contraseña, cuenta los
        //    intentos fallidos (bloqueo temporal) y crea la cookie de autenticación.
        var resultado = await _servicio.IniciarSesionAsync(modelo.Correo, modelo.Clave, modelo.Recordarme);

        // 3. Si falló: se muestra el mensaje que devolvió el servicio. Ese mensaje es
        //    genérico a propósito ("Correo o contraseña incorrectos.") para no revelar
        //    si el correo existe en el sistema.
        if (!resultado.Exitoso)
        {
            foreach (var error in resultado.Errores)
            {
                ModelState.AddModelError(string.Empty, error);
            }
            return View(modelo);
        }

        // 4. Éxito. Si venía de una página protegida, vuelve a ella; pero SOLO si la
        //    dirección es local (Url.IsLocalUrl). Esto evita el ataque de "open redirect",
        //    donde un enlace malicioso enviaría al usuario a otro sitio tras el login.
        if (!string.IsNullOrEmpty(modelo.UrlRetorno) && Url.IsLocalUrl(modelo.UrlRetorno))
        {
            return LocalRedirect(modelo.UrlRetorno);
        }

        return RedirectToAction(nameof(AreaPersonal));
    }

    // ==================== CIERRE DE SESIÓN ====================

    /// <summary>
    /// POST /Cuenta/Logout: cierra la sesión. Se hace por POST (y no por un enlace GET)
    /// con token antiforgery, para que otro sitio no pueda cerrar la sesión del
    /// usuario sin que éste lo sepa.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        // Borra la cookie de autenticación: sin ella ya no se puede acceder a
        // las páginas protegidas.
        await _servicio.CerrarSesionAsync();

        TempData["Mensaje"] = "Sesión cerrada correctamente.";
        return RedirectToAction("Index", "Home");
    }

    // ==================== ÁREA PERSONAL ====================

    /// <summary>
    /// GET /Cuenta/AreaPersonal: página que solo ven los usuarios autenticados.
    /// [Authorize] la protege EN EL SERVIDOR: sin sesión, el sistema redirige a
    /// /Cuenta/Login automáticamente. Ocultar el enlace del menú no basta;
    /// esta etiqueta es la que realmente impide el acceso.
    /// </summary>
    [Authorize]
    [HttpGet]
    public IActionResult AreaPersonal()
    {
        return View();
    }

    // ==================== ACCESO DENEGADO ====================

    /// <summary>
    /// GET /Cuenta/AccesoDenegado: se muestra cuando un usuario autenticado intenta
    /// entrar a algo para lo que no tiene permiso (por ejemplo, un Cliente en una
    /// página de administrador). Responde con el código HTTP 403 (Prohibido).
    /// </summary>
    [HttpGet]
    public IActionResult AccesoDenegado()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View();
    }
}