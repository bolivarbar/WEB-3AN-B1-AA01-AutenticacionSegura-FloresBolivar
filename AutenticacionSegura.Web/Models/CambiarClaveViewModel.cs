using System.ComponentModel.DataAnnotations;

namespace AutenticacionSegura.Web.Models;

/// <summary>
/// Modelo del formulario "Cambiar contraseña" (vista Perfil/CambiarClave).
/// Exige la contraseña actual, la nueva y su confirmación.
/// </summary>
public class CambiarClaveViewModel
{
    /// <summary>Contraseña actual; el servidor la verifica antes de cambiar nada.</summary>
    [Required(ErrorMessage = "Ingrese su contraseña actual.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña actual")]
    public string ClaveActual { get; set; } = string.Empty;

    /// <summary>Contraseña nueva; las reglas de complejidad las valida Identity.</summary>
    [Required(ErrorMessage = "Ingrese la nueva contraseña.")]
    [DataType(DataType.Password)]
    [Display(Name = "Nueva contraseña")]
    public string ClaveNueva { get; set; } = string.Empty;

    /// <summary>Repetición de la nueva contraseña para evitar errores de tipeo.</summary>
    [Required(ErrorMessage = "Confirme la nueva contraseña.")]
    [DataType(DataType.Password)]
    [Compare(nameof(ClaveNueva), ErrorMessage = "La confirmación no coincide con la nueva contraseña.")]
    [Display(Name = "Confirmar nueva contraseña")]
    public string ConfirmarClave { get; set; } = string.Empty;
}