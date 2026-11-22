using AutenticacionSegura.Negocio.Modelos;

namespace AutenticacionSegura.Negocio.Servicios;

/// <summary>
/// Contrato del servicio de autenticación. El controlador depende de esta
/// interfaz y no de la implementación, lo que mantiene las capas separadas
/// y permite cambiar o probar la lógica sin tocar la Presentación.
/// En pasos siguientes se agregarán aquí inicio de sesión, recuperación, etc.
/// </summary>
public interface IServicioAutenticacion
{
    /// <summary>
    /// Registra una cuenta nueva. Nota de seguridad: este método NO recibe
    /// el rol; todo usuario nuevo queda como Cliente.
    /// </summary>
    Task<ResultadoOperacion> RegistrarAsync(string nombreCompleto, string correo, string clave);
}