using Biblioteca.Entidades;
using Microsoft.Data.SqlClient;

namespace Biblioteca.Datos;

public class EditorialDatos
{
    private static Editorial Mapear(SqlDataReader r) => new Editorial(
        r.Entero("IdEditorial"),
        r.Texto("Nombre"),
        r.Texto("Pais"),
        r.Texto("Telefono"));

    public List<Editorial> Listar(string filtro = null)
    {
        const string sql = @"
            SELECT IdEditorial, Nombre, Pais, Telefono
            FROM Editoriales
            WHERE Nombre LIKE @Like OR ISNULL(Pais, '') LIKE @Like
            ORDER BY Nombre";
        return Ejecutor.Consultar(sql, Mapear, Ejecutor.P("@Like", "%" + (filtro ?? string.Empty).Trim() + "%"));
    }

    public Editorial Obtener(int id)
    {
        const string sql = "SELECT IdEditorial, Nombre, Pais, Telefono FROM Editoriales WHERE IdEditorial = @Id";
        return Ejecutor.Consultar(sql, Mapear, Ejecutor.P("@Id", id)).FirstOrDefault();
    }

    public bool Existe(int id)
    {
        const string sql = "SELECT COUNT(1) FROM Editoriales WHERE IdEditorial = @Id";
        return (int)Ejecutor.Escalar(sql, Ejecutor.P("@Id", id)) > 0;
    }

    public int Insertar(Editorial editorial)
    {
        const string sql = @"
            INSERT INTO Editoriales (Nombre, Pais, Telefono)
            OUTPUT INSERTED.IdEditorial
            VALUES (@Nombre, @Pais, @Telefono)";
        return (int)Ejecutor.Escalar(sql,
            Ejecutor.PTexto("@Nombre", editorial.Nombre),
            Ejecutor.PTexto("@Pais", editorial.Pais),
            Ejecutor.PTexto("@Telefono", editorial.Telefono));
    }

    public bool Actualizar(Editorial editorial)
    {
        const string sql = @"
            UPDATE Editoriales
            SET Nombre = @Nombre, Pais = @Pais, Telefono = @Telefono
            WHERE IdEditorial = @Id";
        return Ejecutor.NoQuery(sql,
            Ejecutor.P("@Id", editorial.Id),
            Ejecutor.PTexto("@Nombre", editorial.Nombre),
            Ejecutor.PTexto("@Pais", editorial.Pais),
            Ejecutor.PTexto("@Telefono", editorial.Telefono)) > 0;
    }

    public bool Eliminar(int id)
    {
        const string sql = "DELETE FROM Editoriales WHERE IdEditorial = @Id";
        return Ejecutor.NoQuery(sql, Ejecutor.P("@Id", id)) > 0;
    }
}
