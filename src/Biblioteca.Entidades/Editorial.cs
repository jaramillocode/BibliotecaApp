namespace Biblioteca.Entidades;

public class Editorial : EntidadBase
{
    public string Nombre { get; set; } = string.Empty;
    public string Pais { get; set; }
    public string Telefono { get; set; }

    public Editorial()
    {
    }

    public Editorial(int id, string nombre, string pais, string telefono) : base(id)
    {
        Nombre = nombre;
        Pais = pais;
        Telefono = telefono;
    }

    public override string ToString() => Nombre;
}
