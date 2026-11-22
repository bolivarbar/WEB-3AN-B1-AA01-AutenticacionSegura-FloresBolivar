namespace AutenticacionSegura.Negocio.Modelos;

/// <summary>
/// Respuesta estándar de las operaciones de negocio: dice si salió bien
/// y, si no, la lista de errores en español. Así la capa de Presentación
/// (los controladores) no necesita conocer detalles de Identity.
/// </summary>
public class ResultadoOperacion
{
    /// <summary>True si la operación se realizó correctamente.</summary>
    public bool Exitoso { get; private set; }

    /// <summary>Mensajes de error para mostrar al usuario (vacía si fue exitosa).</summary>
    public IReadOnlyList<string> Errores { get; private set; } = Array.Empty<string>();

    /// <summary>Crea un resultado exitoso.</summary>
    public static ResultadoOperacion Ok() => new() { Exitoso = true };

    /// <summary>Crea un resultado fallido con varios mensajes de error.</summary>
    public static ResultadoOperacion Fallo(IEnumerable<string> errores) =>
        new() { Exitoso = false, Errores = errores.ToList() };

    /// <summary>Crea un resultado fallido con un solo mensaje de error.</summary>
    public static ResultadoOperacion Fallo(string error) => Fallo(new[] { error });
}