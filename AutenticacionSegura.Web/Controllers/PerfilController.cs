using System.Security.Claims;
using AutenticacionSegura.Negocio.Servicios;
using AutenticacionSegura.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutenticacionSegura.Web.Controllers;

/// <summary>
/// Controlador del perfil del usuario autenticado.
/// [Authorize] a nivel de clase: sin sesión iniciada nadie entra a ninguna acción;
/// el sistema redirige al login. Cualquier rol (Cliente o Administrador) puede usarlo.
/// Cada usuario solo ve y modifica SU propio perfil: el id sale de la cookie,
/// nunca de un parámetro que el usuario pueda manipular.
/// </summary>
[Authorize]
public class PerfilController : Controller
{
    private readonly IServicioPerfil _servicioPerfil;

    public PerfilController(IServicioPerfil servicioPerfil)
    {
        _servicioPerfil = servicioPerfil;
    }

    /// <summary>Obtiene el id del usuario autenticado desde los claims de su cookie.</summary>
    private string IdUsuarioActual() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    // ---------------------------------------------------------------
    // FORMULARIO "EDITAR PERFIL" (vista Perfil/Index)
    // ---------------------------------------------------------------

    /// <summary>GET /Perfil: muestra los datos del usuario y el formulario de edición.</summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var usuario = await _servicioPerfil.ObtenerPerfilAsync(IdUsuarioActual());
        if (usuario is null) return RedirectToAction("Login", "Cuenta");

        var modelo = new EditarPerfilViewModel { NombreCompleto = usuario.NombreCompleto };
        CargarDatosSoloLectura(modelo, usuario.Email, usuario.FechaRegistro);
        return View(modelo);
    }

    /// <summary>POST /Perfil: guarda el nuevo nombre del usuario.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(EditarPerfilViewModel modelo)
    {
        var usuario = await _servicioPerfil.ObtenerPerfilAsync(IdUsuarioActual());
        if (usuario is null) return RedirectToAction("Login", "Cuenta");

        // Los campos de solo lectura no vienen del formulario: se vuelven a cargar.
        CargarDatosSoloLectura(modelo, usuario.Email, usuario.FechaRegistro);

        if (!ModelState.IsValid) return View(modelo);

        var resultado = await _servicioPerfil.ActualizarNombreAsync(IdUsuarioActual(), modelo.NombreCompleto);
        if (!resultado.Exitoso)
        {
            // Errores es una lista: se muestra cada mensaje en el resumen de validación.
            foreach (var error in resultado.Errores)
                ModelState.AddModelError(string.Empty, error);
            return View(modelo);
        }

        TempData["Mensaje"] = "Su perfil se actualizó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // ---------------------------------------------------------------
    // FORMULARIO "CAMBIAR CONTRASEÑA" (vista Perfil/CambiarClave)
    // ---------------------------------------------------------------

    /// <summary>GET /Perfil/CambiarClave: muestra el formulario vacío.</summary>
    [HttpGet]
    public IActionResult CambiarClave() => View(new CambiarClaveViewModel());

    /// <summary>
    /// POST /Perfil/CambiarClave: cambia la contraseña. Si la actual es incorrecta,
    /// el servicio no modifica nada y aquí se muestra el error.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarClave(CambiarClaveViewModel modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        var resultado = await _servicioPerfil.CambiarClaveAsync(
            IdUsuarioActual(), modelo.ClaveActual, modelo.ClaveNueva);

        if (!resultado.Exitoso)
        {
            // Errores es una lista: se muestra cada mensaje en el resumen de validación.
            foreach (var error in resultado.Errores)
                ModelState.AddModelError(string.Empty, error);
            return View(modelo);   // las contraseñas no se devuelven a la vista (campos Password)
        }

        TempData["Mensaje"] = "Su contraseña se cambió correctamente.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Rellena los datos informativos (no editables) del modelo del perfil.</summary>
    private void CargarDatosSoloLectura(EditarPerfilViewModel modelo, string? correo, DateTime fechaRegistro)
    {
        modelo.Correo = correo ?? string.Empty;
        modelo.FechaRegistro = fechaRegistro;
        modelo.Rol = User.IsInRole("Administrador") ? "Administrador" : "Cliente";
    }
}