using AutenticacionSegura.Negocio.Modelos;

namespace AutenticacionSegura.Negocio.Servicios;

/// <summary>
/// Contrato del servicio de autenticación. El controlador depende de esta
/// interfaz y no de la implementación, lo que mantiene las capas separadas.
/// </summary>
public interface IServicioAutenticacion
{
    /// <summary>
    /// Registra una cuenta nueva. NO recibe el rol: todo usuario nuevo queda como Cliente.
    /// </summary>
    Task<ResultadoOperacion> RegistrarAsync(string nombreCompleto, string correo, string clave);

    /// <summary>
    /// Inicia sesión con correo y contraseña. Ante cualquier fallo de credenciales
    /// devuelve un mensaje genérico, para no revelar si el correo existe.
    /// </summary>
    /// <param name="recordarme">Si es true, la cookie es persistente (sobrevive al cerrar el navegador).</param>
    Task<ResultadoOperacion> IniciarSesionAsync(string correo, string clave, bool recordarme);

    /// <summary>Cierra la sesión actual: invalida la cookie de autenticación.</summary>
    Task CerrarSesionAsync();
}