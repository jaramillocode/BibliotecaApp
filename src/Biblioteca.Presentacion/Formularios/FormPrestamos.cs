using Biblioteca.Entidades;
using Biblioteca.Negocio;

namespace Biblioteca.Presentacion.Formularios;

/// <summary>
/// Préstamos y devoluciones. Para registrar uno nuevo se elige usuario, libro y fecha.
/// Al seleccionar un préstamo existente se puede corregir su fecha, registrar la devolución o eliminarlo.
/// </summary>
public class FormPrestamos : FormModulo
{
    private readonly PrestamoNegocio _negocio = new();
    private readonly UsuarioNegocio _usuarios = new();
    private readonly LibroNegocio _libros = new();

    private readonly ComboBox _cboUsuario;
    private readonly ComboBox _cboLibro;
    private readonly DateTimePicker _dtpPrestamo;
    private readonly DateTimePicker _dtpDevolucion;
    private readonly TextBox _txtEstado;
    private readonly ComboBox _cboFiltroEstado;
    private readonly Button _botonDevolver;

    public FormPrestamos() : base("Préstamos y devoluciones", "Datos del préstamo")
    {
        _cboUsuario = Campo("Usuario *", new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList }, 0);
        _cboLibro = Campo("Libro *", new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList }, 1);
        _dtpPrestamo = Campo("Fecha del préstamo *", new DateTimePicker
        {
            Format = DateTimePickerFormat.Short,
            MinDate = new DateTime(2000, 1, 1),
            MaxDate = DateTime.Today
        }, 2);
        _dtpDevolucion = Campo("Fecha de devolución", new DateTimePicker
        {
            Format = DateTimePickerFormat.Short,
            MinDate = new DateTime(2000, 1, 1),
            MaxDate = DateTime.Today,
            Enabled = false
        }, 3);
        _txtEstado = Campo("Estado", new TextBox { ReadOnly = true }, 4);

        _botonDevolver = AgregarBotonExtra("Registrar devolución", Ui.ColorPrimario, (s, e) => Seguro(RegistrarDevolucion));
        _botonDevolver.Enabled = false;

        // Filtro por estado junto al buscador
        PanelBusqueda.Controls.Add(new Label { Text = "Estado:", AutoSize = true, Left = 330, Top = 14 });
        _cboFiltroEstado = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Left = 390,
            Top = 10,
            Width = 130
        };
        _cboFiltroEstado.Items.AddRange(new object[] { "Todos", EstadoPrestamo.Prestado, EstadoPrestamo.Devuelto });
        _cboFiltroEstado.SelectedIndex = 0;
        PanelBusqueda.Controls.Add(_cboFiltroEstado);
        _cboFiltroEstado.SelectedIndexChanged += (s, e) =>
        {
            if (IsHandleCreated)
            {
                Seguro(Refrescar);
            }
        };

        // Colores para distinguir préstamos activos de devueltos
        var fuenteNegrita = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        Disposed += (s, e) => fuenteNegrita.Dispose();
        Tabla.CellFormatting += (s, e) =>
        {
            if (e.RowIndex < 0 || e.Value == null)
            {
                return;
            }

            if (Tabla.Columns[e.ColumnIndex].DataPropertyName == "Estado")
            {
                e.CellStyle.ForeColor = e.Value.ToString() == EstadoPrestamo.Prestado
                    ? Color.FromArgb(200, 110, 0)
                    : Ui.ColorExito;
                e.CellStyle.Font = fuenteNegrita;
            }
        };
    }

    protected override string PreguntaEliminar =>
        "¿Desea eliminar este préstamo? Si todavía estaba activo, el ejemplar volverá a quedar disponible.";

    protected override void CargarInicial()
    {
        RecargarListas();
        LimpiarCampos();
    }

    private void RecargarListas()
    {
        _cboUsuario.DisplayMember = "Descripcion";
        _cboUsuario.ValueMember = "Id";
        _cboUsuario.DataSource = _usuarios.Listar();

        _cboLibro.DisplayMember = "Descripcion";
        _cboLibro.ValueMember = "Id";
        _cboLibro.DataSource = _libros.Listar();
    }

    protected override void CargarGrid(string filtro)
    {
        string estado = _cboFiltroEstado.SelectedIndex <= 0 ? null : _cboFiltroEstado.SelectedItem.ToString();
        Enlazar(_negocio.Listar(estado, filtro));
    }

    protected override void ConfigurarColumnas()
    {
        Ocultar("IdUsuario", "IdLibro");
        Encabezado("Id", "Código");
        Encabezado("FechaPrestamo", "Fecha préstamo");
        Encabezado("FechaDevolucion", "Fecha devolución");
        Encabezado("Estado", "Estado");
        Encabezado("NombreUsuario", "Usuario");
        Encabezado("Documento", "Documento");
        Encabezado("TituloLibro", "Libro");
        Ordenar("Id", "NombreUsuario", "Documento", "TituloLibro", "FechaPrestamo", "FechaDevolucion", "Estado");
    }

    protected override void MostrarSeleccion(object registro)
    {
        var prestamo = (Prestamo)registro;

        // El usuario y el libro de un préstamo existente no se cambian.
        _cboUsuario.SelectedValue = prestamo.IdUsuario;
        _cboLibro.SelectedValue = prestamo.IdLibro;
        _cboUsuario.Enabled = false;
        _cboLibro.Enabled = false;

        Ui.PonerFecha(_dtpPrestamo, prestamo.FechaPrestamo);
        _dtpDevolucion.Enabled = true;
        Ui.PonerFecha(_dtpDevolucion, prestamo.FechaDevolucion ?? DateTime.Today);

        _txtEstado.Text = prestamo.Estado;
        _botonDevolver.Enabled = prestamo.EstaActivo;
    }

    protected override string GuardarRegistro()
    {
        if (IdSeleccionado == null)
        {
            int idUsuario = _cboUsuario.SelectedValue is int u ? u : 0;
            int idLibro = _cboLibro.SelectedValue is int l ? l : 0;
            _negocio.Registrar(idUsuario, idLibro, _dtpPrestamo.Value);
            RecargarListas();
            return "El préstamo se registró correctamente.";
        }

        // Al corregir un préstamo activo, la fecha de devolución del selector no se guarda (todavía no existe).
        bool estaDevuelto = _txtEstado.Text == EstadoPrestamo.Devuelto;
        DateTime? devolucion = estaDevuelto ? _dtpDevolucion.Value : (DateTime?)null;
        _negocio.Actualizar(IdSeleccionado.Value, _dtpPrestamo.Value, devolucion);
        return "El préstamo se actualizó correctamente.";
    }

    private void RegistrarDevolucion()
    {
        if (IdSeleccionado == null)
        {
            Ui.Advertencia("Seleccione en la tabla el préstamo que desea devolver.");
            return;
        }

        _negocio.Devolver(IdSeleccionado.Value, _dtpDevolucion.Value);
        RecargarListas();
        Ui.Info("La devolución se registró correctamente. El ejemplar quedó disponible nuevamente.");
        Refrescar();
        Nuevo();
    }

    protected override string EliminarRegistro()
    {
        _negocio.Eliminar(IdSeleccionado.Value);
        RecargarListas();
        return "El préstamo se eliminó correctamente.";
    }

    protected override void LimpiarCampos()
    {
        _cboUsuario.Enabled = true;
        _cboLibro.Enabled = true;
        _cboUsuario.SelectedIndex = -1;
        _cboLibro.SelectedIndex = -1;
        Ui.PonerFecha(_dtpPrestamo, DateTime.Today);
        Ui.PonerFecha(_dtpDevolucion, DateTime.Today);
        _dtpDevolucion.Enabled = false;
        _txtEstado.Text = "(préstamo nuevo)";
        _botonDevolver.Enabled = false;
    }
}
