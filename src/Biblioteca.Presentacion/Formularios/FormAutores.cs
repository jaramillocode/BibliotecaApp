using Biblioteca.Entidades;
using Biblioteca.Negocio;

namespace Biblioteca.Presentacion.Formularios;

public class FormAutores : FormModulo
{
    private readonly AutorNegocio _negocio = new();
    private readonly TextBox _txtNombre;
    private readonly TextBox _txtApellido;
    private readonly TextBox _txtNacionalidad;

    public FormAutores() : base("Gestión de autores", "Datos del autor")
    {
        _txtNombre = Campo("Nombre *", new TextBox { MaxLength = 80 }, 0);
        _txtApellido = Campo("Apellido *", new TextBox { MaxLength = 80 }, 1);
        _txtNacionalidad = Campo("Nacionalidad", new TextBox { MaxLength = 60 }, 2);
    }

    protected override string PreguntaEliminar =>
        "¿Desea eliminar este autor? Solo se puede si no tiene libros registrados.";

    protected override void CargarGrid(string filtro)
    {
        Enlazar(_negocio.Listar(filtro));
    }

    protected override void ConfigurarColumnas()
    {
        Encabezado("Id", "Código");
        Encabezado("Nombre", "Nombre");
        Encabezado("Apellido", "Apellido");
        Encabezado("Nacionalidad", "Nacionalidad");
        Ordenar("Id", "Nombre", "Apellido", "Nacionalidad");
    }

    protected override void MostrarSeleccion(object registro)
    {
        var autor = (Autor)registro;
        _txtNombre.Text = autor.Nombre;
        _txtApellido.Text = autor.Apellido;
        _txtNacionalidad.Text = autor.Nacionalidad;
    }

    protected override string GuardarRegistro()
    {
        var autor = new Autor(IdSeleccionado ?? 0, _txtNombre.Text, _txtApellido.Text, _txtNacionalidad.Text);

        if (IdSeleccionado == null)
        {
            _negocio.Registrar(autor);
            return "El autor se registró correctamente.";
        }

        _negocio.Actualizar(autor);
        return "El autor se actualizó correctamente.";
    }

    protected override string EliminarRegistro()
    {
        _negocio.Eliminar(IdSeleccionado.Value);
        return "El autor se eliminó correctamente.";
    }

    protected override void LimpiarCampos()
    {
        _txtNombre.Clear();
        _txtApellido.Clear();
        _txtNacionalidad.Clear();
        _txtNombre.Focus();
    }
}
