namespace Biblioteca.Entidades;

public class Usuario : Persona
{
    public string Documento { get; set; } = string.Empty;
    public string Telefono { get; set; }
    public string Correo { get; set; }

    public Usuario()
    {
    }

    public Usuario(int id, string nombre, string apellido, string documento, string telefono, string correo)
        : base(id, nombre, apellido)
    {
        Documento = documento;
        Telefono = telefono;
        Correo = correo;
    }

    /// <summary>Texto para mostrar en listas desplegables: "documento - nombre".</summary>
    public string Descripcion => $"{Documento} - {NombreCompleto}";
}
