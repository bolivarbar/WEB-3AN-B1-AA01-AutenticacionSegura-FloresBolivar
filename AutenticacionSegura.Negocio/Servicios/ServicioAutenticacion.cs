using AutenticacionSegura.Datos.Entidades;
using AutenticacionSegura.Negocio.Modelos;
using Microsoft.AspNetCore.Identity;

namespace AutenticacionSegura.Negocio.Servicios;

/// <summary>
/// Reglas de negocio de autenticación. Usa UserManager y SignInManager de
/// ASP.NET Core Identity; no se implementa ningún algoritmo criptográfico propio.
/// </summary>
public class ServicioAutenticacion : IServicioAutenticacion
{
    private readonly UserManager<Usuario> _gestorUsuarios;
    private readonly SignInManager<Usuario> _gestorSesion;

    // Ambos gestores llegan por inyección de dependencias (los registra AddIdentity).
    public ServicioAutenticacion(UserManager<Usuario> gestorUsuarios, SignInManager<Usuario> gestorSesion)
    {
        _gestorUsuarios = gestorUsuarios;
        _gestorSesion = gestorSesion;
    }

    public async Task<ResultadoOperacion> RegistrarAsync(string nombreCompleto, string correo, string clave)
    {
        // 1. Campos obligatorios (validación del lado del servidor).
        if (string.IsNullOrWhiteSpace(nombreCompleto))
            return ResultadoOperacion.Fallo("El nombre completo es obligatorio.");

        if (string.IsNullOrWhiteSpace(correo))
            return ResultadoOperacion.Fallo("El correo es obligatorio.");

        if (string.IsNullOrWhiteSpace(clave))
            return ResultadoOperacion.Fallo("La contraseña es obligatoria.");

        correo = correo.Trim();

        // 2. Correo único.
        if (await _gestorUsuarios.FindByEmailAsync(correo) is not null)
            return ResultadoOperacion.Fallo("Ese correo ya está registrado.");

        // 3. Se arma el usuario. El correo también sirve de nombre de usuario.
        var usuario = new Usuario
        {
            UserName = correo,
            Email = correo,
            NombreCompleto = nombreCompleto.Trim(),
            Activo = true
        };

        // 4. CreateAsync valida la política de contraseña y la guarda con hash.
        var resultado = await _gestorUsuarios.CreateAsync(usuario, clave);
        if (!resultado.Succeeded)
            return ResultadoOperacion.Fallo(resultado.Errors.Select(e => e.Description));

        // 5. Todo usuario nuevo recibe SIEMPRE el rol Cliente.
        var rolAsignado = await _gestorUsuarios.AddToRoleAsync(usuario, RolesSistema.Cliente);
        if (!rolAsignado.Succeeded)
        {
            await _gestorUsuarios.DeleteAsync(usuario);
            return ResultadoOperacion.Fallo("No se pudo completar el registro. Intente nuevamente.");
        }

        return ResultadoOperacion.Ok();
    }

    public async Task<ResultadoOperacion> IniciarSesionAsync(string correo, string clave, bool recordarme)
    {
        // Mensaje único para todos los fallos de credenciales: así nadie puede
        // averiguar qué correos existen en el sistema.
        const string mensajeGenerico = "Correo o contraseña incorrectos.";

        if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(clave))
            return ResultadoOperacion.Fallo(mensajeGenerico);

        var usuario = await _gestorUsuarios.FindByEmailAsync(correo.Trim());
        if (usuario is null)
            return ResultadoOperacion.Fallo(mensajeGenerico);

        // Verifica la contraseña contra el hash. lockoutOnFailure: true suma un intento
        // fallido y, al llegar al máximo configurado, bloquea la cuenta temporalmente.
        var resultado = await _gestorSesion.CheckPasswordSignInAsync(usuario, clave, lockoutOnFailure: true);

        if (resultado.IsLockedOut)
            return ResultadoOperacion.Fallo("Demasiados intentos fallidos. La cuenta está bloqueada temporalmente; intente de nuevo más tarde.");

        if (!resultado.Succeeded)
            return ResultadoOperacion.Fallo(mensajeGenerico);

        // Cuenta deshabilitada por un administrador: no entra (mismo mensaje genérico).
        if (!usuario.Activo)
            return ResultadoOperacion.Fallo(mensajeGenerico);

        // Crea la cookie de autenticación. isPersistent = "Recordarme":
        //  - false: cookie de sesión (se borra al cerrar el navegador)
        //  - true : cookie persistente con la vigencia configurada
        await _gestorSesion.SignInAsync(usuario, isPersistent: recordarme);
        return ResultadoOperacion.Ok();
    }

    public async Task CerrarSesionAsync()
    {
        // Elimina la cookie de autenticación: sin ella no se puede seguir accediendo.
        await _gestorSesion.SignOutAsync();
    }
}