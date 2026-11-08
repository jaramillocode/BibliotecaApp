using Biblioteca.Entidades;
using Biblioteca.Negocio;

namespace Biblioteca.Presentacion.Formularios;

public class FormLibros : FormModulo
{
    private readonly LibroNegocio _negocio = new();
    private readonly AutorNegocio _autores = new();
    private readonly EditorialNegocio _editoriales = new();

    private readonly TextBox _txtIsbn;
    private readonly TextBox _txtTitulo;
    private readonly ComboBox _cboAutor;
    private readonly ComboBox _cboEditorial;
    private readonly NumericUpDown _nudAnio;
    private readonly NumericUpDown _nudExistencias;
    private readonly ComboBox _cboCriterio;

    public FormLibros() : base("Gestión de libros", "Datos del libro")
    {
        _txtIsbn = Campo("ISBN *", new TextBox { MaxLength = 17 }, 0);
        _txtTitulo = Campo("Título *", new TextBox { MaxLength = 200 }, 1);
        _cboAutor = Campo("Autor *", new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList }, 2);
        _cboEditorial = Campo("Editorial *", new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList }, 3);
        _nudAnio = Campo("Año de publicación *", new NumericUpDown
        {
            Minimum = 1000,
            Maximum = DateTime.Today.Year,
            Value = DateTime.Today.Year
        }, 4);
        _nudExistencias = Campo("Existencias (total de ejemplares) *", new NumericUpDown
        {
            Minimum = 0,
            Maximum = 9999,
            Value = 1
        }, 5);

        // Criterio de búsqueda: código, título o autor (RF01)
        PanelBusqueda.Controls.Add(new Label { Text = "en:", AutoSize = true, Left = 330, Top = 14 });
        _cboCriterio = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Left = 358,
            Top = 10,
            Width = 190
        };
        _cboCriterio.Items.AddRange(new object[] { "Todos los campos", "Código / ISBN", "Título", "Autor" });
        _cboCriterio.SelectedIndex = 0;
        PanelBusqueda.Controls.Add(_cboCriterio);
        _cboCriterio.SelectedIndexChanged += (s, e) =>
        {
            if (IsHandleCreated)
            {
                Seguro(Refrescar);
            }
        };
    }

    protected override string PreguntaEliminar =>
        "¿Desea eliminar este libro? Solo se puede si no tiene préstamos asociados.";

    protected override void CargarInicial()
    {
        RecargarListas();
    }

    private void RecargarListas()
    {
        _cboAutor.DisplayMember = "NombreCompleto";
        _cboAutor.ValueMember = "Id";
        _cboAutor.DataSource = _autores.Listar();
        _cboAutor.SelectedIndex = -1;

        _cboEditorial.DisplayMember = "Nombre";
        _cboEditorial.ValueMember = "Id";
        _cboEditorial.DataSource = _editoriales.Listar();
        _cboEditorial.SelectedIndex = -1;
    }

    private CriterioBusquedaLibro CriterioActual()
    {
        switch (_cboCriterio.SelectedIndex)
        {
            case 1: return CriterioBusquedaLibro.Codigo;
            case 2: return CriterioBusquedaLibro.Titulo;
            case 3: return CriterioBusquedaLibro.Autor;
            default: return CriterioBusquedaLibro.Todos;
        }
    }

    protected override void CargarGrid(string filtro)
    {
        Enlazar(_negocio.Buscar(CriterioActual(), filtro));
    }

    protected override void ConfigurarColumnas()
    {
        Ocultar("IdAutor", "IdEditorial");
        Encabezado("Id", "Código");
        Encabezado("Isbn", "ISBN");
        Encabezado("Titulo", "Título");
        Encabezado("NombreAutor", "Autor");
        Encabezado("NombreEditorial", "Editorial");
        Encabezado("AnioPublicacion", "Año");
        Encabezado("Existencias", "Existencias");
        Encabezado("Disponibles", "Disponibles");
        Ordenar("Id", "Isbn", "Titulo", "NombreAutor", "NombreEditorial", "AnioPublicacion", "Existencias", "Disponibles");
    }

    protected override void MostrarSeleccion(object registro)
    {
        var libro = (Libro)registro;
        _txtIsbn.Text = libro.Isbn;
        _txtTitulo.Text = libro.Titulo;
        _cboAutor.SelectedValue = libro.IdAutor;
        _cboEditorial.SelectedValue = libro.IdEditorial;
        _nudAnio.Value = Math.Min(Math.Max(libro.AnioPublicacion, (int)_nudAnio.Minimum), (int)_nudAnio.Maximum);
        _nudExistencias.Value = Math.Min(Math.Max(libro.Existencias, (int)_nudExistencias.Minimum), (int)_nudExistencias.Maximum);
    }

    protected override string GuardarRegistro()
    {
        int idAutor = _cboAutor.SelectedValue is int a ? a : 0;
        int idEditorial = _cboEditorial.SelectedValue is int e ? e : 0;

        var libro = new Libro(IdSeleccionado ?? 0, _txtIsbn.Text, _txtTitulo.Text, idAutor, idEditorial,
            (int)_nudAnio.Value, (int)_nudExistencias.Value);

        if (IdSeleccionado == null)
        {
            _negocio.Registrar(libro);
            return "El libro se registró correctamente.";
        }

        _negocio.Actualizar(libro);
        return "El libro se actualizó correctamente.";
    }

    protected override string EliminarRegistro()
    {
        _negocio.Eliminar(IdSeleccionado.Value);
        return "El libro se eliminó correctamente.";
    }

    protected override void LimpiarCampos()
    {
        _txtIsbn.Clear();
        _txtTitulo.Clear();
        _cboAutor.SelectedIndex = -1;
        _cboEditorial.SelectedIndex = -1;
        _nudAnio.Value = _nudAnio.Maximum;
        _nudExistencias.Value = 1;
        _txtIsbn.Focus();
    }
}
