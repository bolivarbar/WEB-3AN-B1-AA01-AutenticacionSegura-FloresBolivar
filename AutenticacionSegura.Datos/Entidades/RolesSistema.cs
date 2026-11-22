namespace AutenticacionSegura.Datos.Entidades;

/// <summary>
/// Nombres de los roles del sistema, definidos en un solo lugar.
/// Se usan "const" (constantes) para evitar errores de escritura:
/// si en un controlador se escribiera "Administrator" en vez de
/// "Administrador", el rol no coincidiría y el acceso fallaría.
/// Más adelante se usarán así: [Authorize(Roles = RolesSistema.Administrador)]
/// </summary>
public static class RolesSistema
{
    /// <summary>Rol por defecto de todo usuario que se registra.</summary>
    public const string Cliente = "Cliente";

    /// <summary>Rol con permisos administrativos. Solo se asigna por seeding o por otro administrador.</summary>
    public const string Administrador = "Administrador";
}