using System.Data;

namespace Biblioteca.Datos;

/// <summary>
/// Consultas del módulo de reportes. Devuelven DataTable porque cada reporte tiene columnas distintas
/// y no corresponde a una sola entidad. Dos de ellas usan las vistas definidas en el script SQL.
/// </summary>
public class ReporteDatos
{
    public DataTable ListadoLibros() => Ejecutor.Tabla(@"
        SELECT Isbn, Titulo, Autor, Editorial, AnioPublicacion AS [Año], Existencias, Disponibles
        FROM vw_Libros
        ORDER BY Titulo");

    public DataTable ListadoAutores() => Ejecutor.Tabla(@"
        SELECT a.IdAutor AS [Código], a.Nombre, a.Apellido, a.Nacionalidad,
               COUNT(l.IdLibro) AS [Libros registrados]
        FROM Autores a
        LEFT JOIN Libros l ON l.IdAutor = a.IdAutor
        GROUP BY a.IdAutor, a.Nombre, a.Apellido, a.Nacionalidad
        ORDER BY a.Apellido, a.Nombre");

    public DataTable ListadoEditoriales() => Ejecutor.Tabla(@"
        SELECT e.IdEditorial AS [Código], e.Nombre, e.Pais AS [País], e.Telefono AS [Teléfono],
               COUNT(l.IdLibro) AS [Libros registrados]
        FROM Editoriales e
        LEFT JOIN Libros l ON l.IdEditorial = e.IdEditorial
        GROUP BY e.IdEditorial, e.Nombre, e.Pais, e.Telefono
        ORDER BY e.Nombre");

    public DataTable ListadoUsuarios() => Ejecutor.Tabla(@"
        SELECT u.IdUsuario AS [Código], u.Nombre, u.Apellido, u.Documento, u.Telefono, u.Correo,
               SUM(CASE WHEN p.Estado = 'Prestado' THEN 1 ELSE 0 END) AS [Préstamos activos]
        FROM Usuarios u
        LEFT JOIN Prestamos p ON p.IdUsuario = u.IdUsuario
        GROUP BY u.IdUsuario, u.Nombre, u.Apellido, u.Documento, u.Telefono, u.Correo
        ORDER BY u.Apellido, u.Nombre");

    public DataTable RelacionPrestamos() => Ejecutor.Tabla(@"
        SELECT IdPrestamo AS [Código], Usuario, Documento, Libro, FechaPrestamo AS [Fecha préstamo],
               FechaDevolucion AS [Fecha devolución], Estado
        FROM vw_Prestamos
        ORDER BY IdPrestamo DESC");

    public DataTable PrestamosPorEstado(string estado) => Ejecutor.Tabla(@"
        SELECT IdPrestamo AS [Código], Usuario, Documento, Libro, FechaPrestamo AS [Fecha préstamo],
               FechaDevolucion AS [Fecha devolución], DiasTranscurridos AS [Días]
        FROM vw_Prestamos
        WHERE Estado = @Estado
        ORDER BY FechaPrestamo DESC",
        Ejecutor.P("@Estado", estado));

    public DataTable HistorialPrestamos() => Ejecutor.Tabla(@"
        SELECT Usuario, Documento, Libro, FechaPrestamo AS [Fecha préstamo],
               FechaDevolucion AS [Fecha devolución], Estado
        FROM vw_Prestamos
        ORDER BY Usuario, FechaPrestamo DESC");

    public DataTable LibrosMasPrestados() => Ejecutor.Tabla(@"
        SELECT l.Titulo, a.Nombre + ' ' + a.Apellido AS Autor, COUNT(p.IdPrestamo) AS [Veces prestado]
        FROM Libros l
        INNER JOIN Autores a ON a.IdAutor = l.IdAutor
        LEFT JOIN Prestamos p ON p.IdLibro = l.IdLibro
        GROUP BY l.IdLibro, l.Titulo, a.Nombre, a.Apellido
        ORDER BY COUNT(p.IdPrestamo) DESC, l.Titulo");

    public DataTable ResumenGeneral() => Ejecutor.Tabla(@"
        SELECT 1 AS Orden, N'Títulos registrados' AS Concepto, CAST(COUNT(*) AS INT) AS Valor FROM Libros
        UNION ALL SELECT 2, N'Ejemplares en total', CAST(ISNULL(SUM(Existencias), 0) AS INT) FROM Libros
        UNION ALL SELECT 3, N'Ejemplares disponibles', CAST(ISNULL(SUM(Disponibles), 0) AS INT) FROM Libros
        UNION ALL SELECT 4, N'Autores', CAST(COUNT(*) AS INT) FROM Autores
        UNION ALL SELECT 5, N'Editoriales', CAST(COUNT(*) AS INT) FROM Editoriales
        UNION ALL SELECT 6, N'Usuarios', CAST(COUNT(*) AS INT) FROM Usuarios
        UNION ALL SELECT 7, N'Préstamos activos', CAST(COUNT(*) AS INT) FROM Prestamos WHERE Estado = 'Prestado'
        UNION ALL SELECT 8, N'Préstamos devueltos', CAST(COUNT(*) AS INT) FROM Prestamos WHERE Estado = 'Devuelto'
        ORDER BY Orden");
}
