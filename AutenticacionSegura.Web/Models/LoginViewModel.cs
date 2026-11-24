using System.ComponentModel.DataAnnotations;

namespace AutenticacionSegura.Web.Models;

/// <summary>
/// Datos del formulario de inicio de sesión. Los atributos hacen que el
/// formulario valide que los campos no estén vacíos y que el correo tenga
/// formato válido; el servidor vuelve a comprobarlo con ModelState.IsValid.
/// </summary>
public class LoginViewModel
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [Display(Name = "Correo electrónico")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Clave { get; set; } = string.Empty;

    // "Recordarme": si está marcado, la cookie de autenticación es persistente
    // y sigue vigente aunque se cierre el navegador (hasta su fecha de caducidad).
    [Display(Name = "Recordarme")]
    public bool Recordarme { get; set; }

    // Dirección a la que se vuelve después de iniciar sesión
    // (la llena el sistema cuando alguien intentó entrar a una página protegida).
    public string? UrlRetorno { get; set; }
}