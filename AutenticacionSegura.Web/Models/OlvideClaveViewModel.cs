using System.ComponentModel.DataAnnotations;

namespace AutenticacionSegura.Web.Models;

/// <summary>
/// Datos del formulario "Olvidé mi contraseña": solo pide el correo.
/// </summary>
public class OlvideClaveViewModel
{
    [Required(ErrorMessage = "Ingrese su correo electrónico.")]
    [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
    [Display(Name = "Correo electrónico")]
    public string Correo { get; set; } = string.Empty;
}