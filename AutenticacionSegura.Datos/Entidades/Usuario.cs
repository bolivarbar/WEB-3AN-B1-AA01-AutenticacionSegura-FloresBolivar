// "using" importa el espacio de nombres donde vive IdentityUser,
// la clase base de ASP.NET Core Identity para representar usuarios.
using Microsoft.AspNetCore.Identity;

// "namespace" agrupa las clases del proyecto. Esta línea indica que
// Usuario pertenece a la capa de Datos, carpeta Entidades.
namespace AutenticacionSegura.Datos.Entidades;

/// <summary>
/// Representa a un usuario del sistema (cliente o administrador).
/// Hereda de IdentityUser, que ya incluye los campos de seguridad:
/// Id, UserName, Email, PasswordHash (contraseña cifrada con hash),
/// bloqueo por intentos fallidos (LockoutEnd, AccessFailedCount), etc.
/// Aquí solo agregamos los datos propios de nuestra aplicación.
/// </summary>
public class Usuario : IdentityUser
{
    /// <summary>
    /// Nombre completo que la persona escribe al registrarse.
    /// Se inicializa en vacío para evitar valores nulos.
    /// </summary>
    public string NombreCompleto { get; set; } = string.Empty;

    /// <summary>
    /// Indica si la cuenta está habilitada. El administrador podrá
    /// deshabilitar cuentas (requisito de la actividad). Por defecto
    /// una cuenta nueva nace activa.
    /// </summary>
    public bool Activo { get; set; } = true;

    /// <summary>
    /// Fecha y hora de creación de la cuenta. Se guarda en UTC
    /// (hora universal) para que no dependa de la zona horaria
    /// del servidor.
    /// </summary>
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
}