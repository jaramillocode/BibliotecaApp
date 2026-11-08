using Biblioteca.Entidades;
using Microsoft.Data.SqlClient;

namespace Biblioteca.Datos;

public class AutorDatos
{
    private static Autor Mapear(SqlDataReader r) => new Autor(
        r.Entero("IdAutor"),
        r.Texto("Nombre"),
        r.Texto("Apellido"),
        r.Texto("Nacionalidad"));

    public List<Autor> Listar(string filtro = null)
    {
        const string sql = @"
            SELECT IdAutor, Nombre, Apellido, Nacionalidad
            FROM Autores
            WHERE Nombre LIKE @Like OR Apellido LIKE @Like OR ISNULL(Nacionalidad, '') LIKE @Like
            ORDER BY Apellido, Nombre";
        return Ejecutor.Consultar(sql, Mapear, Ejecutor.P("@Like", "%" + (filtro ?? string.Empty).Trim() + "%"));
    }

    public Autor Obtener(int id)
    {
        const string sql = "SELECT IdAutor, Nombre, Apellido, Nacionalidad FROM Autores WHERE IdAutor = @Id";
        return Ejecutor.Consultar(sql, Mapear, Ejecutor.P("@Id", id)).FirstOrDefault();
    }

    public bool Existe(int id)
    {
        const string sql = "SELECT COUNT(1) FROM Autores WHERE IdAutor = @Id";
        return (int)Ejecutor.Escalar(sql, Ejecutor.P("@Id", id)) > 0;
    }

    public int Insertar(Autor autor)
    {
        const string sql = @"
            INSERT INTO Autores (Nombre, Apellido, Nacionalidad)
            OUTPUT INSERTED.IdAutor
            VALUES (@Nombre, @Apellido, @Nacionalidad)";
        return (int)Ejecutor.Escalar(sql,
            Ejecutor.PTexto("@Nombre", autor.Nombre),
            Ejecutor.PTexto("@Apellido", autor.Apellido),
            Ejecutor.PTexto("@Nacionalidad", autor.Nacionalidad));
    }

    public bool Actualizar(Autor autor)
    {
        const string sql = @"
            UPDATE Autores
            SET Nombre = @Nombre, Apellido = @Apellido, Nacionalidad = @Nacionalidad
            WHERE IdAutor = @Id";
        return Ejecutor.NoQuery(sql,
            Ejecutor.P("@Id", autor.Id),
            Ejecutor.PTexto("@Nombre", autor.Nombre),
            Ejecutor.PTexto("@Apellido", autor.Apellido),
            Ejecutor.PTexto("@Nacionalidad", autor.Nacionalidad)) > 0;
    }

    public bool Eliminar(int id)
    {
        const string sql = "DELETE FROM Autores WHERE IdAutor = @Id";
        return Ejecutor.NoQuery(sql, Ejecutor.P("@Id", id)) > 0;
    }
}
