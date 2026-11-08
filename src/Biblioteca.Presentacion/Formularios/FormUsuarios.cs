using Biblioteca.Entidades;
using Biblioteca.Negocio;

namespace Biblioteca.Presentacion.Formularios;

public class FormUsuarios : FormModulo
{
    private readonly UsuarioNegocio _negocio = new();
    private readonly TextBox _txtNombre;
    private readonly TextBox _txtApellido;
    private readonly TextBox _txtDocumento;
    private readonly TextBox _txtTelefono;
    private readonly TextBox _txtCorreo;

    public FormUsuarios() : base("Gestión de usuarios", "Datos del usuario")
    {
        _txtNombre = Campo("Nombre *", new TextBox { MaxLength = 80 }, 0);
        _txtApellido = Campo("Apellido *", new TextBox { MaxLength = 80 }, 1);
        _txtDocumento = Campo("Documento *", new TextBox { MaxLength = 20 }, 2);
        _txtTelefono = Campo("Teléfono", new TextBox { MaxLength = 20 }, 3);
        _txtCorreo = Campo("Correo electrónico", new TextBox { MaxLength = 120 }, 4);
    }

    protected override string PreguntaEliminar =>
        "¿Desea eliminar este usuario? Solo se puede si no tiene préstamos registrados.";

    protected override void CargarGrid(string filtro)
    {
        Enlazar(_negocio.Listar(filtro));
    }

    protected override void ConfigurarColumnas()
    {
        Encabezado("Id", "Código");
        Encabezado("Telefono", "Teléfono");
        Encabezado("Correo", "Correo electrónico");
        Ordenar("Id", "Documento", "Nombre", "Apellido", "Telefono", "Correo");
    }

    protected override void MostrarSeleccion(object registro)
    {
        var usuario = (Usuario)registro;
        _txtNombre.Text = usuario.Nombre;
        _txtApellido.Text = usuario.Apellido;
        _txtDocumento.Text = usuario.Documento;
        _txtTelefono.Text = usuario.Telefono;
        _txtCorreo.Text = usuario.Correo;
    }

    protected override string GuardarRegistro()
    {
        var usuario = new Usuario(IdSeleccionado ?? 0, _txtNombre.Text, _txtApellido.Text,
            _txtDocumento.Text, _txtTelefono.Text, _txtCorreo.Text);

        if (IdSeleccionado == null)
        {
            _negocio.Registrar(usuario);
            return "El usuario se registró correctamente.";
        }

        _negocio.Actualizar(usuario);
        return "El usuario se actualizó correctamente.";
    }

    protected override string EliminarRegistro()
    {
        _negocio.Eliminar(IdSeleccionado.Value);
        return "El usuario se eliminó correctamente.";
    }

    protected override void LimpiarCampos()
    {
        _txtNombre.Clear();
        _txtApellido.Clear();
        _txtDocumento.Clear();
        _txtTelefono.Clear();
        _txtCorreo.Clear();
        _txtNombre.Focus();
    }
}
