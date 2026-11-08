using Biblioteca.Datos;
using Biblioteca.Entidades;

namespace Biblioteca.Negocio;

public class PrestamoNegocio
{
    private readonly PrestamoDatos _datos = new();
    private readonly UsuarioDatos _usuarios = new();
    private readonly LibroDatos _libros = new();

    public List<Prestamo> Listar(string estado, string texto)
    {
        try { return _datos.Listar(estado, texto); }
        catch (DatosException ex) { throw TraductorErrores.General(ex); }
    }

    /// <summary>Registra un préstamo nuevo: valida usuario, libro, fecha y disponibilidad.</summary>
    public int Registrar(int idUsuario, int idLibro, DateTime fechaPrestamo)
    {
        fechaPrestamo = fechaPrestamo.Date;
        if (fechaPrestamo > DateTime.Today)
        {
            throw new ReglaNegocioException("La fecha del préstamo no puede ser posterior a hoy.");
        }

        try
        {
            if (idUsuario <= 0 || !_usuarios.Existe(idUsuario))
            {
                throw new ReglaNegocioException("Seleccione un usuario que exista en el sistema.");
            }

            Libro libro = idLibro > 0 ? _libros.Obtener(idLibro) : null;
            if (libro == null)
            {
                throw new ReglaNegocioException("Seleccione un libro que exista en el sistema.");
            }

            if (!libro.HayDisponibilidad)
            {
                throw new ReglaNegocioException($"No hay ejemplares disponibles de \"{libro.Titulo}\" en este momento.");
            }

            return _datos.Registrar(new Prestamo(0, idUsuario, idLibro, fechaPrestamo));
        }
        catch (DatosException ex)
        {
            switch (ex.Tipo)
            {
                case TipoErrorDatos.Duplicado:
                    throw new ReglaNegocioException("Este usuario ya tiene ese libro prestado y todavía no lo ha devuelto.", ex);
                case TipoErrorDatos.SinDisponibilidad:
                    throw new ReglaNegocioException("El libro se quedó sin ejemplares disponibles. Actualice la lista.", ex);
                default:
                    throw TraductorErrores.AlGuardar(ex, "El préstamo ya existe.");
            }
        }
    }

    /// <summary>Registra la devolución de un préstamo activo.</summary>
    public void Devolver(int idPrestamo, DateTime fechaDevolucion)
    {
        fechaDevolucion = fechaDevolucion.Date;
        try
        {
            Prestamo prestamo = _datos.Obtener(idPrestamo);
            if (prestamo == null)
            {
                throw new ReglaNegocioException("El préstamo ya no existe. Actualice la lista e intente de nuevo.");
            }

            if (!prestamo.EstaActivo)
            {
                throw new ReglaNegocioException("Este préstamo ya fue devuelto.");
            }

            ValidarFechaDevolucion(prestamo.FechaPrestamo, fechaDevolucion);
            _datos.Devolver(idPrestamo, fechaDevolucion);
        }
        catch (DatosException ex)
        {
            throw TraductorErrores.AlGuardar(ex, "No se pudo registrar la devolución.");
        }
    }

    /// <summary>
    /// Corrige las fechas de un préstamo. El usuario y el libro no se pueden cambiar
    /// (para cambiarlos se elimina el préstamo y se registra uno nuevo), así el inventario nunca queda descuadrado.
    /// </summary>
    public void Actualizar(int idPrestamo, DateTime fechaPrestamo, DateTime? fechaDevolucion)
    {
        fechaPrestamo = fechaPrestamo.Date;
        if (fechaPrestamo > DateTime.Today)
        {
            throw new ReglaNegocioException("La fecha del préstamo no puede ser posterior a hoy.");
        }

        try
        {
            Prestamo prestamo = _datos.Obtener(idPrestamo);
            if (prestamo == null)
            {
                throw new ReglaNegocioException("El préstamo ya no existe. Actualice la lista e intente de nuevo.");
            }

            DateTime? devolucion = null;
            if (!prestamo.EstaActivo)
            {
                if (fechaDevolucion == null)
                {
                    throw new ReglaNegocioException("Un préstamo devuelto debe tener fecha de devolución.");
                }

                devolucion = fechaDevolucion.Value.Date;
                ValidarFechaDevolucion(fechaPrestamo, devolucion.Value);
            }

            _datos.ActualizarFechas(idPrestamo, fechaPrestamo, devolucion);
        }
        catch (DatosException ex)
        {
            throw TraductorErrores.AlGuardar(ex, "No se pudo actualizar el préstamo.");
        }
    }

    /// <summary>Elimina un préstamo. Si estaba activo, el ejemplar vuelve a quedar disponible.</summary>
    public void Eliminar(int idPrestamo)
    {
        try { _datos.Eliminar(idPrestamo); }
        catch (DatosException ex) { throw TraductorErrores.AlEliminar(ex, "No se pudo eliminar el préstamo."); }
    }

    private static void ValidarFechaDevolucion(DateTime fechaPrestamo, DateTime fechaDevolucion)
    {
        if (fechaDevolucion < fechaPrestamo)
        {
            throw new ReglaNegocioException("La fecha de devolución no puede ser anterior a la fecha del préstamo.");
        }

        if (fechaDevolucion > DateTime.Today)
        {
            throw new ReglaNegocioException("La fecha de devolución no puede ser posterior a hoy.");
        }
    }
}
