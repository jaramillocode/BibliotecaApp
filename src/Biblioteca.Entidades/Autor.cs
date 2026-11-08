namespace Biblioteca.Entidades;

public class Autor : Persona
{
    public string Nacionalidad { get; set; }

    public Autor()
    {
    }

    public Autor(int id, string nombre, string apellido, string nacionalidad)
        : base(id, nombre, apellido)
    {
        Nacionalidad = nacionalidad;
    }

    /// <summary>
    /// Polimorfismo: los autores sobrescriben ToString para mostrar también la nacionalidad
    /// cuando se conoce (la clase base solo muestra el nombre completo).
    /// </summary>
    public override string ToString() =>
        string.IsNullOrWhiteSpace(Nacionalidad) ? NombreCompleto : $"{NombreCompleto} ({Nacionalidad})";
}
