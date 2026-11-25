using System.ComponentModel.DataAnnotations;

namespace AutenticacionSegura.Web.Models;

/// <summary>
/// Modelo del formulario "Editar perfil" (vista Perfil/Index).
/// Solo el nombre es editable; el correo y la fecha de registro se muestran
/// únicamente como información y no se reciben desde el formulario.
/// </summary>
public class EditarPerfilViewModel
{
    /// <summary>Nombre completo editable por el usuario.</summary>
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    [Display(Name = "Nombre completo")]
    public string NombreCompleto { get; set; } = string.Empty;

    /// <summary>Correo del usuario (solo lectura, se rellena desde la base).</summary>
    [Display(Name = "Correo electrónico")]
    public string Correo { get; set; } = string.Empty;

    /// <summary>Fecha de registro (solo lectura, se rellena desde la base).</summary>
    [Display(Name = "Fecha de registro")]
    public DateTime FechaRegistro { get; set; }

    /// <summary>Rol del usuario (solo lectura, se rellena desde la base).</summary>
    [Display(Name = "Rol")]
    public string Rol { get; set; } = string.Empty;
}