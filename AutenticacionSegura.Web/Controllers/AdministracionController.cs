using System.Security.Claims;
using AutenticacionSegura.Datos.Entidades;
using AutenticacionSegura.Negocio.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutenticacionSegura.Negocio.Modelos;

namespace AutenticacionSegura.Web.Controllers;

/// <summary>
/// Administración de usuarios: listado, cambio de rol y habilitar/deshabilitar.
/// [Authorize(Roles = Administrador)] a nivel de clase: el servidor rechaza a cualquiera
/// que no sea administrador. Un Cliente autenticado recibe "Acceso denegado" (403) y un
/// visitante sin sesión es enviado al login. Ocultar el enlace en el menú es solo comodidad;
/// esta es la protección real (prueba 8 del PDF).
/// Además, ServicioAdministracion vuelve a comprobar el rol del actor en la base de datos.
/// </summary>
[Authorize(Roles = RolesSistema.Administrador)]
public class AdministracionController : Controller
{
    private readonly IServicioAdministracion _servicioAdministracion;

    public AdministracionController(IServicioAdministracion servicioAdministracion)
    {
        _servicioAdministracion = servicioAdministracion;
    }

    /// <summary>Obtiene el id del administrador que ejecuta la acción (desde su cookie).</summary>
    private string IdActual() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    // ---------------------------------------------------------------
    // LISTADO DE USUARIOS (vista Administracion/Index)
    // ---------------------------------------------------------------

    /// <summary>GET /Administracion: tabla con todos los usuarios, su rol y su estado.</summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var usuarios = await _servicioAdministracion.ListarUsuariosAsync();
        return View(usuarios);
    }

    // ---------------------------------------------------------------
    // CAMBIAR ROL
    // ---------------------------------------------------------------

    /// <summary>
    /// POST /Administracion/CambiarRol: asigna "Cliente" o "Administrador" a un usuario.
    /// Se usa POST + token antifalsificación porque modifica datos.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarRol(string id, string rol)
    {
        var resultado = await _servicioAdministracion.CambiarRolAsync(IdActual(), id, rol);
        RegistrarMensaje(resultado, "El rol se actualizó correctamente.");
        return RedirectToAction(nameof(Index));
    }

    // ---------------------------------------------------------------
    // HABILITAR / DESHABILITAR
    // ---------------------------------------------------------------

    /// <summary>POST /Administracion/CambiarEstado: habilita (true) o deshabilita (false) una cuenta.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(string id, bool activo)
    {
        var resultado = await _servicioAdministracion.CambiarEstadoAsync(IdActual(), id, activo);
        RegistrarMensaje(resultado, activo
            ? "La cuenta se habilitó correctamente."
            : "La cuenta se deshabilitó correctamente.");
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Deja en TempData el mensaje que se mostrará tras la redirección:
    /// "Mensaje" (verde) si salió bien, "Error" (rojo) con los motivos si falló.
    /// </summary>
    private void RegistrarMensaje(ResultadoOperacion resultado, string mensajeExito)
    
    {
        if (resultado.Exitoso)
            TempData["Mensaje"] = mensajeExito;
        else
            TempData["Error"] = string.Join(" ", resultado.Errores);
    }
}