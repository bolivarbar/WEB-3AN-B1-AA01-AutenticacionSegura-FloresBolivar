using System.ComponentModel.DataAnnotations;

namespace AutenticacionSegura.Web.Models;

/// <summary>
/// Datos del formulario para crear la nueva contraseña.
/// Correo y Token llegan en el enlace del correo y viajan como campos ocultos.
/// </summary>
public class RestablecerClaveViewModel
{
    /// <summary>Correo de la cuenta (viene en el enlace; campo oculto).</summary>
    public string Correo { get; set; } = string.Empty;

    /// <summary>Token de restablecimiento (viene en el enlace; campo oculto).</summary>
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingrese la nueva contraseña.")]
    [DataType(DataType.Password)]
    [Display(Name = "Nueva contraseña")]
    public string Clave { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirme la nueva contraseña.")]
    [DataType(DataType.Password)]
    [Compare("Clave", ErrorMessage = "Las contraseñas no coinciden.")]
    [Display(Name = "Confirmar contraseña")]
    public string ConfirmarClave { get; set; } = string.Empty;
}