using Biblioteca.Datos;
using Biblioteca.Entidades;

namespace Biblioteca.Negocio;

public class EditorialNegocio
{
    private readonly EditorialDatos _datos = new();

    public List<Editorial> Listar(string filtro = null)
    {
        try { return _datos.Listar(filtro); }
        catch (DatosException ex) { throw TraductorErrores.General(ex); }
    }

    public int Registrar(Editorial editorial)
    {
        Validar(editorial);
        try { return _datos.Insertar(editorial); }
        catch (DatosException ex) { throw TraductorErrores.AlGuardar(ex, "Ya existe una editorial con ese nombre."); }
    }

    public void Actualizar(Editorial editorial)
    {
        Validar(editorial);
        try
        {
            if (!_datos.Actualizar(editorial))
            {
                throw new ReglaNegocioException("La editorial ya no existe. Actualice la lista e intente de nuevo.");
            }
        }
        catch (DatosException ex) { throw TraductorErrores.AlGuardar(ex, "Ya existe una editorial con ese nombre."); }
    }

    public void Eliminar(int id)
    {
        try
        {
            if (!_datos.Eliminar(id))
            {
                throw new ReglaNegocioException("La editorial ya no existe. Actualice la lista e intente de nuevo.");
            }
        }
        catch (DatosException ex)
        {
            throw TraductorErrores.AlEliminar(ex, "No se puede eliminar la editorial porque tiene libros registrados.");
        }
    }

    public bool Existe(int id)
    {
        try { return _datos.Existe(id); }
        catch (DatosException ex) { throw TraductorErrores.General(ex); }
    }

    private static void Validar(Editorial editorial)
    {
        if (editorial == null)
        {
            throw new ReglaNegocioException("No se recibieron los datos de la editorial.");
        }

        editorial.Nombre = Validador.Requerido(editorial.Nombre, "El nombre de la editorial", 100);
        editorial.Pais = Validador.Opcional(editorial.Pais, "El país", 60);
        editorial.Telefono = Validador.Telefono(editorial.Telefono);
    }
}
