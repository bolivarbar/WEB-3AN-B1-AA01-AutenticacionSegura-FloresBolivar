using AutenticacionSegura.Negocio.Servicios;
using AutenticacionSegura.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace AutenticacionSegura.Web.Controllers;

/// <summary>
/// Controlador de cuentas. Solo coordina: recibe el formulario, llama al
/// servicio de Negocio y decide qué vista mostrar. NO contiene reglas de
/// negocio ni consultas a la base de datos (arquitectura de 3 capas).
/// </summary>
public class CuentaController : Controller
{
    private readonly IServicioAutenticacion _servicio;

    // El servicio llega por inyección de dependencias (registrado en Program.cs).
    public CuentaController(IServicioAutenticacion servicio)
    {
        _servicio = servicio;
    }

    // GET /Cuenta/Registro -> muestra el formulario vacío.
    [HttpGet]
    public IActionResult Registro()
    {
        return View();
    }

    // POST /Cuenta/Registro -> procesa el formulario.
    // [ValidateAntiForgeryToken] rechaza envíos que no traigan el token antiforgery
    // (protección contra ataques CSRF).
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registro(RegistroViewModel modelo)
    {
        // 1. Validaciones del modelo en el servidor (campos obligatorios, formato, confirmación).
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        // 2. La lógica real la hace la capa de Negocio.
        var resultado = await _servicio.RegistrarAsync(modelo.NombreCompleto, modelo.Correo, modelo.Clave);

        // 3. Si falló (correo duplicado, contraseña débil...), se muestran los errores en la vista.
        if (!resultado.Exitoso)
        {
            foreach (var error in resultado.Errores)
            {
                ModelState.AddModelError(string.Empty, error);
            }
            return View(modelo);
        }

        // 4. Éxito: se guarda un mensaje temporal y se redirige.
        // (Cuando exista el login, la redirección irá a la pantalla de inicio de sesión.)
        TempData["Mensaje"] = "Cuenta creada correctamente. Ya puede iniciar sesión.";
        return RedirectToAction("Index", "Home");
    }
}