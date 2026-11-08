namespace Biblioteca.Entidades;

/// <summary>
/// Datos comunes de las personas que maneja la biblioteca (autores y usuarios).
/// Aquí se justifica la herencia: ambos comparten nombre, apellido y nombre completo.
/// </summary>
public abstract class Persona : EntidadBase
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;

    protected Persona()
    {
    }

    protected Persona(int id, string nombre, string apellido) : base(id)
    {
        Nombre = nombre;
        Apellido = apellido;
    }

    /// <summary>Nombre completo; las clases hijas pueden cambiar la forma de armarlo.</summary>
    public virtual string NombreCompleto => $"{Nombre} {Apellido}".Trim();

    public override string ToString() => NombreCompleto;
}
