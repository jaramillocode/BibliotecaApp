using Biblioteca.Datos;
using Biblioteca.Entidades;

namespace Biblioteca.Negocio;

public class UsuarioNegocio
{
    private readonly UsuarioDatos _datos = new();

    public List<Usuario> Listar(string filtro = null)
    {
        try { return _datos.Listar(filtro); }
        catch (DatosException ex) { throw TraductorErrores.General(ex); }
    }

    public int Registrar(Usuario usuario)
    {
        Validar(usuario);
        try { return _datos.Insertar(usuario); }
        catch (DatosException ex) { throw TraductorErrores.AlGuardar(ex, "Ya existe un usuario con ese número de documento."); }
    }

    public void Actualizar(Usuario usuario)
    {
        Validar(usuario);
        try
        {
            if (!_datos.Actualizar(usuario))
            {
                throw new ReglaNegocioException("El usuario ya no existe. Actualice la lista e intente de nuevo.");
            }
        }
        catch (DatosException ex) { throw TraductorErrores.AlGuardar(ex, "Ya existe un usuario con ese número de documento."); }
    }

    public void Eliminar(int id)
    {
        try
        {
            if (!_datos.Eliminar(id))
            {
                throw new ReglaNegocioException("El usuario ya no existe. Actualice la lista e intente de nuevo.");
            }
        }
        catch (DatosException ex)
        {
            throw TraductorErrores.AlEliminar(ex, "No se puede eliminar el usuario porque tiene préstamos registrados.");
        }
    }

    public bool Existe(int id)
    {
        try { return _datos.Existe(id); }
        catch (DatosException ex) { throw TraductorErrores.General(ex); }
    }

    private static void Validar(Usuario usuario)
    {
        if (usuario == null)
        {
            throw new ReglaNegocioException("No se recibieron los datos del usuario.");
        }

        usuario.Nombre = Validador.Requerido(usuario.Nombre, "El nombre", 80);
        usuario.Apellido = Validador.Requerido(usuario.Apellido, "El apellido", 80);
        usuario.Documento = Validador.Documento(usuario.Documento);
        usuario.Telefono = Validador.Telefono(usuario.Telefono);
        usuario.Correo = Validador.Correo(usuario.Correo);
    }
}
