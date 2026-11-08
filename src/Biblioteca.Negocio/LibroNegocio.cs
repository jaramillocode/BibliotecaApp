using Biblioteca.Datos;
using Biblioteca.Entidades;

namespace Biblioteca.Negocio;

public class LibroNegocio
{
    private const string MensajeIsbnDuplicado = "Ya existe un libro registrado con ese ISBN.";

    private readonly LibroDatos _datos = new();
    private readonly AutorDatos _autores = new();
    private readonly EditorialDatos _editoriales = new();

    public List<Libro> Buscar(CriterioBusquedaLibro criterio, string texto)
    {
        try { return _datos.Buscar(criterio, texto); }
        catch (DatosException ex) { throw TraductorErrores.General(ex); }
    }

    public List<Libro> Listar()
    {
        try { return _datos.Listar(); }
        catch (DatosException ex) { throw TraductorErrores.General(ex); }
    }

    public Libro Obtener(int id)
    {
        try { return _datos.Obtener(id); }
        catch (DatosException ex) { throw TraductorErrores.General(ex); }
    }

    public int Registrar(Libro libro)
    {
        Validar(libro);
        try { return _datos.Insertar(libro); }
        catch (DatosException ex) { throw TraductorErrores.AlGuardar(ex, MensajeIsbnDuplicado); }
    }

    public void Actualizar(Libro libro)
    {
        Validar(libro);
        try
        {
            Libro actual = _datos.Obtener(libro.Id);
            if (actual == null)
            {
                throw new ReglaNegocioException("El libro ya no existe. Actualice la lista e intente de nuevo.");
            }

            // No se pueden dejar menos ejemplares en total que los que hoy están prestados.
            if (libro.Existencias < actual.Prestados)
            {
                throw new ReglaNegocioException(
                    $"No se pueden dejar {libro.Existencias} existencias: hay {actual.Prestados} ejemplares prestados en este momento.");
            }

            _datos.Actualizar(libro);
        }
        catch (DatosException ex) { throw TraductorErrores.AlGuardar(ex, MensajeIsbnDuplicado); }
    }

    public void Eliminar(int id)
    {
        try
        {
            if (!_datos.Eliminar(id))
            {
                throw new ReglaNegocioException("El libro ya no existe. Actualice la lista e intente de nuevo.");
            }
        }
        catch (DatosException ex)
        {
            throw TraductorErrores.AlEliminar(ex, "No se puede eliminar el libro porque tiene préstamos asociados.");
        }
    }

    private void Validar(Libro libro)
    {
        if (libro == null)
        {
            throw new ReglaNegocioException("No se recibieron los datos del libro.");
        }

        libro.Isbn = Validador.Isbn(libro.Isbn);
        libro.Titulo = Validador.Requerido(libro.Titulo, "El título", 200);

        int anioActual = DateTime.Today.Year;
        if (libro.AnioPublicacion < 1000 || libro.AnioPublicacion > anioActual)
        {
            throw new ReglaNegocioException($"El año de publicación debe estar entre 1000 y {anioActual}.");
        }

        if (libro.Existencias < 0 || libro.Existencias > 9999)
        {
            throw new ReglaNegocioException("Las existencias deben estar entre 0 y 9999.");
        }

        // Las referencias deben apuntar a registros que existan.
        try
        {
            if (libro.IdAutor <= 0 || !_autores.Existe(libro.IdAutor))
            {
                throw new ReglaNegocioException("Seleccione un autor que exista en el sistema.");
            }

            if (libro.IdEditorial <= 0 || !_editoriales.Existe(libro.IdEditorial))
            {
                throw new ReglaNegocioException("Seleccione una editorial que exista en el sistema.");
            }
        }
        catch (DatosException ex)
        {
            throw TraductorErrores.General(ex);
        }
    }
}
