namespace Biblioteca.Entidades;

public class Libro : EntidadBase
{
    public string Isbn { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public int IdAutor { get; set; }
    public int IdEditorial { get; set; }
    public int AnioPublicacion { get; set; }

    /// <summary>Total de ejemplares que tiene la biblioteca.</summary>
    public int Existencias { get; set; }

    /// <summary>Ejemplares que están en el estante (no prestados).</summary>
    public int Disponibles { get; set; }

    // Datos de lectura que llegan de los JOIN con Autores y Editoriales.
    public string NombreAutor { get; set; }
    public string NombreEditorial { get; set; }

    public Libro()
    {
    }

    public Libro(int id, string isbn, string titulo, int idAutor, int idEditorial, int anioPublicacion, int existencias)
        : base(id)
    {
        Isbn = isbn;
        Titulo = titulo;
        IdAutor = idAutor;
        IdEditorial = idEditorial;
        AnioPublicacion = anioPublicacion;
        Existencias = existencias;
        Disponibles = existencias;
    }

    public bool HayDisponibilidad => Disponibles > 0;

    /// <summary>Ejemplares que hoy están prestados.</summary>
    public int Prestados => Existencias - Disponibles;

    /// <summary>Texto para mostrar en listas desplegables.</summary>
    public string Descripcion => $"{Titulo} ({Disponibles} disp.)";

    public override string ToString() => Titulo;
}
