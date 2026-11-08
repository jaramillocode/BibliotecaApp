namespace Biblioteca.Entidades;

/// <summary>Valores permitidos para el estado de un préstamo (coinciden con el CHECK de la base de datos).</summary>
public static class EstadoPrestamo
{
    public const string Prestado = "Prestado";
    public const string Devuelto = "Devuelto";
}

public class Prestamo : EntidadBase
{
    public int IdUsuario { get; set; }
    public int IdLibro { get; set; }
    public DateTime FechaPrestamo { get; set; }
    public DateTime? FechaDevolucion { get; set; }
    public string Estado { get; set; } = EstadoPrestamo.Prestado;

    // Datos de lectura que llegan de los JOIN con Usuarios y Libros.
    public string NombreUsuario { get; set; }
    public string Documento { get; set; }
    public string TituloLibro { get; set; }

    public Prestamo()
    {
    }

    public Prestamo(int id, int idUsuario, int idLibro, DateTime fechaPrestamo) : base(id)
    {
        IdUsuario = idUsuario;
        IdLibro = idLibro;
        FechaPrestamo = fechaPrestamo.Date;
        Estado = EstadoPrestamo.Prestado;
    }

    public bool EstaActivo => Estado == EstadoPrestamo.Prestado;
}
