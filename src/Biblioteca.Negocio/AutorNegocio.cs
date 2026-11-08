using Biblioteca.Datos;
using Biblioteca.Entidades;

namespace Biblioteca.Negocio;

public class AutorNegocio
{
    private readonly AutorDatos _datos = new();

    public List<Autor> Listar(string filtro = null)
    {
        try { return _datos.Listar(filtro); }
        catch (DatosException ex) { throw TraductorErrores.General(ex); }
    }

    public int Registrar(Autor autor)
    {
        Validar(autor);
        try { return _datos.Insertar(autor); }
        catch (DatosException ex) { throw TraductorErrores.AlGuardar(ex, "Ya existe un autor con ese nombre y apellido."); }
    }

    public void Actualizar(Autor autor)
    {
        Validar(autor);
        try
        {
            if (!_datos.Actualizar(autor))
            {
                throw new ReglaNegocioException("El autor ya no existe. Actualice la lista e intente de nuevo.");
            }
        }
        catch (DatosException ex) { throw TraductorErrores.AlGuardar(ex, "Ya existe un autor con ese nombre y apellido."); }
    }

    public void Eliminar(int id)
    {
        try
        {
            if (!_datos.Eliminar(id))
            {
                throw new ReglaNegocioException("El autor ya no existe. Actualice la lista e intente de nuevo.");
            }
        }
        catch (DatosException ex)
        {
            throw TraductorErrores.AlEliminar(ex, "No se puede eliminar el autor porque tiene libros registrados.");
        }
    }

    public bool Existe(int id)
    {
        try { return _datos.Existe(id); }
        catch (DatosException ex) { throw TraductorErrores.General(ex); }
    }

    private static void Validar(Autor autor)
    {
        if (autor == null)
        {
            throw new ReglaNegocioException("No se recibieron los datos del autor.");
        }

        autor.Nombre = Validador.Requerido(autor.Nombre, "El nombre", 80);
        autor.Apellido = Validador.Requerido(autor.Apellido, "El apellido", 80);
        autor.Nacionalidad = Validador.Opcional(autor.Nacionalidad, "La nacionalidad", 60);
    }
}
