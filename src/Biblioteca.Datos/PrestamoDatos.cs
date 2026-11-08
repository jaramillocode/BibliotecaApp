using Biblioteca.Entidades;
using Microsoft.Data.SqlClient;

namespace Biblioteca.Datos;

public class PrestamoDatos
{
    private const string SelectBase = @"
        SELECT p.IdPrestamo, p.IdUsuario, p.IdLibro, p.FechaPrestamo, p.FechaDevolucion, p.Estado,
               u.Nombre + ' ' + u.Apellido AS Usuario,
               u.Documento,
               l.Titulo AS Libro
        FROM Prestamos p
        INNER JOIN Usuarios u ON u.IdUsuario = p.IdUsuario
        INNER JOIN Libros l ON l.IdLibro = p.IdLibro";

    private static Prestamo Mapear(SqlDataReader r) => new Prestamo
    {
        Id = r.Entero("IdPrestamo"),
        IdUsuario = r.Entero("IdUsuario"),
        IdLibro = r.Entero("IdLibro"),
        FechaPrestamo = r.Fecha("FechaPrestamo"),
        FechaDevolucion = r.FechaNula("FechaDevolucion"),
        Estado = r.Texto("Estado"),
        NombreUsuario = r.Texto("Usuario"),
        Documento = r.Texto("Documento"),
        TituloLibro = r.Texto("Libro")
    };

    /// <param name="estado">"Prestado", "Devuelto" o null para ver todos.</param>
    public List<Prestamo> Listar(string estado, string texto)
    {
        string sql = SelectBase + @"
            WHERE (@Estado IS NULL OR p.Estado = @Estado)
              AND (u.Nombre LIKE @Like OR u.Apellido LIKE @Like OR u.Documento LIKE @Like
                   OR l.Titulo LIKE @Like OR l.Isbn LIKE @Like)
            ORDER BY p.IdPrestamo DESC";
        return Ejecutor.Consultar(sql, Mapear,
            Ejecutor.PTexto("@Estado", estado),
            Ejecutor.P("@Like", "%" + (texto ?? string.Empty).Trim() + "%"));
    }

    public Prestamo Obtener(int id)
    {
        string sql = SelectBase + " WHERE p.IdPrestamo = @Id";
        return Ejecutor.Consultar(sql, Mapear, Ejecutor.P("@Id", id)).FirstOrDefault();
    }

    /// <summary>
    /// Registra el préstamo y descuenta un ejemplar disponible en una sola transacción:
    /// o se hacen las dos cosas o no se hace ninguna.
    /// </summary>
    public int Registrar(Prestamo prestamo)
    {
        try
        {
            using var conexion = ConexionBD.CrearConexion();
            conexion.Open();
            using var transaccion = conexion.BeginTransaction();

            using (var descontar = new SqlCommand(
                "UPDATE Libros SET Disponibles = Disponibles - 1 WHERE IdLibro = @IdLibro AND Disponibles > 0",
                conexion, transaccion))
            {
                descontar.Parameters.Add(Ejecutor.P("@IdLibro", prestamo.IdLibro));
                if (descontar.ExecuteNonQuery() == 0)
                {
                    throw new DatosException(TipoErrorDatos.SinDisponibilidad,
                        "El libro no tiene ejemplares disponibles.");
                }
            }

            int id;
            using (var insertar = new SqlCommand(@"
                INSERT INTO Prestamos (IdUsuario, IdLibro, FechaPrestamo, FechaDevolucion, Estado)
                OUTPUT INSERTED.IdPrestamo
                VALUES (@IdUsuario, @IdLibro, @FechaPrestamo, NULL, 'Prestado')",
                conexion, transaccion))
            {
                insertar.Parameters.Add(Ejecutor.P("@IdUsuario", prestamo.IdUsuario));
                insertar.Parameters.Add(Ejecutor.P("@IdLibro", prestamo.IdLibro));
                insertar.Parameters.Add(Ejecutor.PFecha("@FechaPrestamo", prestamo.FechaPrestamo));
                id = (int)insertar.ExecuteScalar();
            }

            transaccion.Commit();
            return id;
        }
        catch (SqlException ex)
        {
            throw Ejecutor.Traducir(ex);
        }
    }

    /// <summary>
    /// Marca el préstamo como devuelto y devuelve el ejemplar al inventario (misma transacción).
    /// </summary>
    public void Devolver(int idPrestamo, DateTime fechaDevolucion)
    {
        try
        {
            using var conexion = ConexionBD.CrearConexion();
            conexion.Open();
            using var transaccion = conexion.BeginTransaction();

            using (var cerrar = new SqlCommand(@"
                UPDATE Prestamos
                SET Estado = 'Devuelto', FechaDevolucion = @Fecha
                WHERE IdPrestamo = @Id AND Estado = 'Prestado'",
                conexion, transaccion))
            {
                cerrar.Parameters.Add(Ejecutor.P("@Id", idPrestamo));
                cerrar.Parameters.Add(Ejecutor.PFecha("@Fecha", fechaDevolucion));
                if (cerrar.ExecuteNonQuery() == 0)
                {
                    throw new DatosException(TipoErrorDatos.NoEncontrado,
                        "El préstamo no existe o ya fue devuelto.");
                }
            }

            using (var reponer = new SqlCommand(@"
                UPDATE Libros
                SET Disponibles = Disponibles + 1
                WHERE IdLibro = (SELECT IdLibro FROM Prestamos WHERE IdPrestamo = @Id)",
                conexion, transaccion))
            {
                reponer.Parameters.Add(Ejecutor.P("@Id", idPrestamo));
                reponer.ExecuteNonQuery();
            }

            transaccion.Commit();
        }
        catch (SqlException ex)
        {
            throw Ejecutor.Traducir(ex);
        }
    }

    /// <summary>Corrige las fechas de un préstamo (el usuario y el libro no se cambian).</summary>
    public bool ActualizarFechas(int idPrestamo, DateTime fechaPrestamo, DateTime? fechaDevolucion)
    {
        const string sql = @"
            UPDATE Prestamos
            SET FechaPrestamo = @FechaPrestamo, FechaDevolucion = @FechaDevolucion
            WHERE IdPrestamo = @Id";
        return Ejecutor.NoQuery(sql,
            Ejecutor.P("@Id", idPrestamo),
            Ejecutor.PFecha("@FechaPrestamo", fechaPrestamo),
            Ejecutor.PFecha("@FechaDevolucion", fechaDevolucion)) > 0;
    }

    /// <summary>
    /// Elimina el préstamo. Si todavía estaba activo, el ejemplar vuelve al inventario
    /// para no dejar el stock descuadrado.
    /// </summary>
    public void Eliminar(int idPrestamo)
    {
        try
        {
            using var conexion = ConexionBD.CrearConexion();
            conexion.Open();
            using var transaccion = conexion.BeginTransaction();

            using (var reponer = new SqlCommand(@"
                UPDATE Libros
                SET Disponibles = Disponibles + 1
                WHERE IdLibro IN (SELECT IdLibro FROM Prestamos WHERE IdPrestamo = @Id AND Estado = 'Prestado')",
                conexion, transaccion))
            {
                reponer.Parameters.Add(Ejecutor.P("@Id", idPrestamo));
                reponer.ExecuteNonQuery();
            }

            using (var borrar = new SqlCommand("DELETE FROM Prestamos WHERE IdPrestamo = @Id", conexion, transaccion))
            {
                borrar.Parameters.Add(Ejecutor.P("@Id", idPrestamo));
                if (borrar.ExecuteNonQuery() == 0)
                {
                    throw new DatosException(TipoErrorDatos.NoEncontrado, "El préstamo ya no existe.");
                }
            }

            transaccion.Commit();
        }
        catch (SqlException ex)
        {
            throw Ejecutor.Traducir(ex);
        }
    }
}
