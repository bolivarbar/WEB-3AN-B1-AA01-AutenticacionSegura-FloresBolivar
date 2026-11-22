using Microsoft.AspNetCore.Identity;

namespace AutenticacionSegura.Datos.Identidad;

/// <summary>
/// Traduce al español los mensajes de error de ASP.NET Core Identity
/// (por defecto vienen en inglés). Cada método devuelve un IdentityError
/// con un código interno (Code) y el texto que verá el usuario (Description).
/// Solo se sobrescriben los errores que pueden aparecer en esta actividad.
/// </summary>
public class DescriptorErroresIdentity : IdentityErrorDescriber
{
    public override IdentityError DefaultError() =>
        new() { Code = nameof(DefaultError), Description = "Ocurrió un error inesperado." };

    public override IdentityError PasswordTooShort(int length) =>
        new() { Code = nameof(PasswordTooShort), Description = $"La contraseña debe tener al menos {length} caracteres." };

    public override IdentityError PasswordRequiresDigit() =>
        new() { Code = nameof(PasswordRequiresDigit), Description = "La contraseña debe incluir al menos un número." };

    public override IdentityError PasswordRequiresLower() =>
        new() { Code = nameof(PasswordRequiresLower), Description = "La contraseña debe incluir al menos una letra minúscula." };

    public override IdentityError PasswordRequiresUpper() =>
        new() { Code = nameof(PasswordRequiresUpper), Description = "La contraseña debe incluir al menos una letra mayúscula." };

    public override IdentityError PasswordRequiresNonAlphanumeric() =>
        new() { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "La contraseña debe incluir al menos un símbolo (por ejemplo # o !)." };

    public override IdentityError PasswordMismatch() =>
        new() { Code = nameof(PasswordMismatch), Description = "La contraseña actual es incorrecta." };

    public override IdentityError DuplicateUserName(string userName) =>
        new() { Code = nameof(DuplicateUserName), Description = "Ese correo ya está registrado." };

    public override IdentityError DuplicateEmail(string email) =>
        new() { Code = nameof(DuplicateEmail), Description = "Ese correo ya está registrado." };

    public override IdentityError InvalidEmail(string? email) =>
        new() { Code = nameof(InvalidEmail), Description = "El correo no tiene un formato válido." };

    public override IdentityError InvalidToken() =>
        new() { Code = nameof(InvalidToken), Description = "El enlace no es válido o ya venció." };
}