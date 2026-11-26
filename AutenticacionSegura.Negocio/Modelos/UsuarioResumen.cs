namespace AutenticacionSegura.Negocio.Modelos;

/// <summary>
/// Datos de un usuario tal como se muestran en el listado de administración.
/// Es un objeto simple (DTO): la capa Web no recibe la entidad Usuario completa
/// con campos internos de Identity (hash de contraseña, sello de seguridad, etc.).
/// </summary>
public class UsuarioResumen
{
    /// <summary>Identificador único del usuario.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Correo electrónico (también es el nombre de inicio de sesión).</summary>
    public string Correo { get; set; } = string.Empty;

    /// <summary>Nombre completo del usuario.</summary>
    public string NombreCompleto { get; set; } = string.Empty;

    /// <summary>Rol actual: "Cliente" o "Administrador".</summary>
    public string Rol { get; set; } = string.Empty;

    /// <summary>true si la cuenta está habilitada; false si fue deshabilitada.</summary>
    public bool Activo { get; set; }

    /// <summary>Fecha de registro en UTC.</summary>
    public DateTime FechaRegistro { get; set; }
}