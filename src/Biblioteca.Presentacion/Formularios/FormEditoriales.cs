using Biblioteca.Entidades;
using Biblioteca.Negocio;

namespace Biblioteca.Presentacion.Formularios;

public class FormEditoriales : FormModulo
{
    private readonly EditorialNegocio _negocio = new();
    private readonly TextBox _txtNombre;
    private readonly TextBox _txtPais;
    private readonly TextBox _txtTelefono;

    public FormEditoriales() : base("Gestión de editoriales", "Datos de la editorial")
    {
        _txtNombre = Campo("Nombre *", new TextBox { MaxLength = 100 }, 0);
        _txtPais = Campo("País", new TextBox { MaxLength = 60 }, 1);
        _txtTelefono = Campo("Teléfono", new TextBox { MaxLength = 20 }, 2);
    }

    protected override string PreguntaEliminar =>
        "¿Desea eliminar esta editorial? Solo se puede si no tiene libros registrados.";

    protected override void CargarGrid(string filtro)
    {
        Enlazar(_negocio.Listar(filtro));
    }

    protected override void ConfigurarColumnas()
    {
        Encabezado("Id", "Código");
        Encabezado("Nombre", "Nombre");
        Encabezado("Pais", "País");
        Encabezado("Telefono", "Teléfono");
        Ordenar("Id", "Nombre", "Pais", "Telefono");
    }

    protected override void MostrarSeleccion(object registro)
    {
        var editorial = (Editorial)registro;
        _txtNombre.Text = editorial.Nombre;
        _txtPais.Text = editorial.Pais;
        _txtTelefono.Text = editorial.Telefono;
    }

    protected override string GuardarRegistro()
    {
        var editorial = new Editorial(IdSeleccionado ?? 0, _txtNombre.Text, _txtPais.Text, _txtTelefono.Text);

        if (IdSeleccionado == null)
        {
            _negocio.Registrar(editorial);
            return "La editorial se registró correctamente.";
        }

        _negocio.Actualizar(editorial);
        return "La editorial se actualizó correctamente.";
    }

    protected override string EliminarRegistro()
    {
        _negocio.Eliminar(IdSeleccionado.Value);
        return "La editorial se eliminó correctamente.";
    }

    protected override void LimpiarCampos()
    {
        _txtNombre.Clear();
        _txtPais.Clear();
        _txtTelefono.Clear();
        _txtNombre.Focus();
    }
}
