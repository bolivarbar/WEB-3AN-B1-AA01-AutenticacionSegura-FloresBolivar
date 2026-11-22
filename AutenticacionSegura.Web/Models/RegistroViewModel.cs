using System.ComponentModel.DataAnnotations;

namespace AutenticacionSegura.Web.Models;

/// <summary>
/// Datos del formulario de registro. Los atributos ([Required], [EmailAddress],
/// [Compare]...) hacen la validación visible en el formulario y también se
/// comprueban en el servidor con ModelState.IsValid.
/// Fíjate en que NO hay un campo "Rol": el usuario no puede elegir su rol.
/// </summary>
public class RegistroViewModel
{
    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    [Display(Name = "Nombre completo")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [Display(Name = "Correo electrónico")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Clave { get; set; } = string.Empty;

    // [Compare] verifica que este campo sea igual a "Clave".
    [Required(ErrorMessage = "Debe confirmar la contraseña.")]
    [DataType(DataType.Password)]
    [Compare("Clave", ErrorMessage = "Las contraseñas no coinciden.")]
    [Display(Name = "Confirmar contraseña")]
    public string ConfirmarClave { get; set; } = string.Empty;
}