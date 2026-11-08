using Biblioteca.Entidades;
using Microsoft.Data.SqlClient;

namespace Biblioteca.Datos;

public class LibroDatos
{
    // Consulta base: trae el libro con el nombre de su autor y de su editorial.
    private const string SelectBase = @"
        SELECT l.IdLibro, l.Isbn, l.Titulo, l.IdAutor, l.IdEditorial, l.AnioPublicacion,
               l.Existencias, l.Disponibles,
               a.Nombre + ' ' + a.Apellido AS Autor,
               e.Nombre AS Editorial
        FROM Libros l
        INNER JOIN Autores a ON a.IdAutor = l.IdAutor
        INNER JOIN Editoriales e ON e.IdEditorial = l.IdEditorial";

    private static Libro Mapear(SqlDataReader r) => new Libro
    {
        Id = r.Entero("IdLibro"),
        Isbn = r.Texto("Isbn"),
        Titulo = r.Texto("Titulo"),
        IdAutor = r.Entero("IdAutor"),
        IdEditorial = r.Entero("IdEditorial"),
        AnioPublicacion = r.Entero("AnioPublicacion"),
        Existencias = r.Entero("Existencias"),
        Disponibles = r.Entero("Disponibles"),
        NombreAutor = r.Texto("Autor"),
        NombreEditorial = r.Texto("Editorial")
    };

    /// <summary>Busca libros según un criterio fijo (el texto siempre viaja como parámetro).</summary>
    public List<Libro> Buscar(CriterioBusquedaLibro criterio, string texto)
    {
        string condicion;
        switch (criterio)
        {
            case CriterioBusquedaLibro.Codigo:
                condicion = "l.Isbn LIKE @Like OR CAST(l.IdLibro AS NVARCHAR(10)) LIKE @Like";
                break;
            case CriterioBusquedaLibro.Titulo:
                condicion = "l.Titulo LIKE @Like";
                break;
            case CriterioBusquedaLibro.Autor:
                condicion = "(a.Nombre + ' ' + a.Apellido) LIKE @Like";
                break;
            default:
                condicion = @"l.Isbn LIKE @Like OR l.Titulo LIKE @Like
                              OR (a.Nombre + ' ' + a.Apellido) LIKE @Like OR e.Nombre LIKE @Like";
                break;
        }

        string sql = SelectBase + " WHERE " + condicion + " ORDER BY l.Titulo";
        return Ejecutor.Consultar(sql, Mapear, Ejecutor.P("@Like", "%" + (texto ?? string.Empty).Trim() + "%"));
    }

    public List<Libro> Listar() => Buscar(CriterioBusquedaLibro.Todos, string.Empty);

    public Libro Obtener(int id)
    {
        string sql = SelectBase + " WHERE l.IdLibro = @Id";
        return Ejecutor.Consultar(sql, Mapear, Ejecutor.P("@Id", id)).FirstOrDefault();
    }

    public int Insertar(Libro libro)
    {
        const string sql = @"
            INSERT INTO Libros (Isbn, Titulo, IdAutor, IdEditorial, AnioPublicacion, Existencias, Disponibles)
            OUTPUT INSERTED.IdLibro
            VALUES (@Isbn, @Titulo, @IdAutor, @IdEditorial, @Anio, @Existencias, @Existencias)";
        return (int)Ejecutor.Escalar(sql,
            Ejecutor.PTexto("@Isbn", libro.Isbn),
            Ejecutor.PTexto("@Titulo", libro.Titulo),
            Ejecutor.P("@IdAutor", libro.IdAutor),
            Ejecutor.P("@IdEditorial", libro.IdEditorial),
            Ejecutor.P("@Anio", libro.AnioPublicacion),
            Ejecutor.P("@Existencias", libro.Existencias));
    }

    /// <summary>
    /// Actualiza el libro. Las existencias nuevas se aplican conservando los ejemplares prestados:
    /// Disponibles = ExistenciasNuevas - (ExistenciasActuales - DisponiblesActuales).
    /// En un UPDATE de SQL Server la parte derecha de cada SET usa los valores anteriores a la modificación.
    /// </summary>
    public bool Actualizar(Libro libro)
    {
        const string sql = @"
            UPDATE Libros
            SET Isbn = @Isbn,
                Titulo = @Titulo,
                IdAutor = @IdAutor,
                IdEditorial = @IdEditorial,
                AnioPublicacion = @Anio,
                Disponibles = @Existencias - (Existencias - Disponibles),
                Existencias = @Existencias
            WHERE IdLibro = @Id";
        return Ejecutor.NoQuery(sql,
            Ejecutor.P("@Id", libro.Id),
            Ejecutor.PTexto("@Isbn", libro.Isbn),
            Ejecutor.PTexto("@Titulo", libro.Titulo),
            Ejecutor.P("@IdAutor", libro.IdAutor),
            Ejecutor.P("@IdEditorial", libro.IdEditorial),
            Ejecutor.P("@Anio", libro.AnioPublicacion),
            Ejecutor.P("@Existencias", libro.Existencias)) > 0;
    }

    public bool Eliminar(int id)
    {
        const string sql = "DELETE FROM Libros WHERE IdLibro = @Id";
        return Ejecutor.NoQuery(sql, Ejecutor.P("@Id", id)) > 0;
    }
}
