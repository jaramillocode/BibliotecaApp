using Biblioteca.Entidades;
using Microsoft.Data.SqlClient;

namespace Biblioteca.Datos;

public class UsuarioDatos
{
    private static Usuario Mapear(SqlDataReader r) => new Usuario(
        r.Entero("IdUsuario"),
        r.Texto("Nombre"),
        r.Texto("Apellido"),
        r.Texto("Documento"),
        r.Texto("Telefono"),
        r.Texto("Correo"));

    private const string Columnas = "IdUsuario, Nombre, Apellido, Documento, Telefono, Correo";

    public List<Usuario> Listar(string filtro = null)
    {
        const string sql = @"
            SELECT " + Columnas + @"
            FROM Usuarios
            WHERE Nombre LIKE @Like OR Apellido LIKE @Like OR Documento LIKE @Like OR ISNULL(Correo, '') LIKE @Like
            ORDER BY Apellido, Nombre";
        return Ejecutor.Consultar(sql, Mapear, Ejecutor.P("@Like", "%" + (filtro ?? string.Empty).Trim() + "%"));
    }

    public Usuario Obtener(int id)
    {
        const string sql = "SELECT " + Columnas + " FROM Usuarios WHERE IdUsuario = @Id";
        return Ejecutor.Consultar(sql, Mapear, Ejecutor.P("@Id", id)).FirstOrDefault();
    }

    public bool Existe(int id)
    {
        const string sql = "SELECT COUNT(1) FROM Usuarios WHERE IdUsuario = @Id";
        return (int)Ejecutor.Escalar(sql, Ejecutor.P("@Id", id)) > 0;
    }

    public int Insertar(Usuario usuario)
    {
        const string sql = @"
            INSERT INTO Usuarios (Nombre, Apellido, Documento, Telefono, Correo)
            OUTPUT INSERTED.IdUsuario
            VALUES (@Nombre, @Apellido, @Documento, @Telefono, @Correo)";
        return (int)Ejecutor.Escalar(sql,
            Ejecutor.PTexto("@Nombre", usuario.Nombre),
            Ejecutor.PTexto("@Apellido", usuario.Apellido),
            Ejecutor.PTexto("@Documento", usuario.Documento),
            Ejecutor.PTexto("@Telefono", usuario.Telefono),
            Ejecutor.PTexto("@Correo", usuario.Correo));
    }

    public bool Actualizar(Usuario usuario)
    {
        const string sql = @"
            UPDATE Usuarios
            SET Nombre = @Nombre, Apellido = @Apellido, Documento = @Documento,
                Telefono = @Telefono, Correo = @Correo
            WHERE IdUsuario = @Id";
        return Ejecutor.NoQuery(sql,
            Ejecutor.P("@Id", usuario.Id),
            Ejecutor.PTexto("@Nombre", usuario.Nombre),
            Ejecutor.PTexto("@Apellido", usuario.Apellido),
            Ejecutor.PTexto("@Documento", usuario.Documento),
            Ejecutor.PTexto("@Telefono", usuario.Telefono),
            Ejecutor.PTexto("@Correo", usuario.Correo)) > 0;
    }

    public bool Eliminar(int id)
    {
        const string sql = "DELETE FROM Usuarios WHERE IdUsuario = @Id";
        return Ejecutor.NoQuery(sql, Ejecutor.P("@Id", id)) > 0;
    }
}
