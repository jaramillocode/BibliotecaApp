using Biblioteca.Entidades;

namespace Biblioteca.Presentacion.Formularios;

/// <summary>
/// Plantilla de los formularios de gestión (CRUD). Dibuja el panel de datos a la izquierda,
/// el buscador y la tabla a la derecha, y los botones Nuevo / Guardar / Eliminar.
/// Cada módulo (libros, autores, ...) hereda de esta clase y solo define sus campos y
/// cómo guardar, eliminar o mostrar un registro.
/// </summary>
public abstract class FormModulo : FormEmbebido
{
    private readonly Label _titulo = new();
    private readonly Panel _panelCentro = new();
    private bool _cargando;

    protected readonly GroupBox GrupoDatos = new();
    protected readonly Panel PanelBotones = new();
    protected readonly Panel PanelBusqueda = new();
    protected readonly TextBox CajaBusqueda = new();
    protected readonly DataGridView Tabla = new();
    protected readonly Button BotonNuevo;
    protected readonly Button BotonGuardar;
    protected readonly Button BotonEliminar;

    /// <summary>Id del registro que se está editando; null cuando se está creando uno nuevo.</summary>
    protected int? IdSeleccionado { get; set; }

    protected FormModulo(string titulo, string textoGrupo)
    {
        // Título
        _titulo.Text = titulo;
        _titulo.Dock = DockStyle.Top;
        _titulo.Height = 56;
        _titulo.Font = new Font("Segoe UI Semibold", 16f);
        _titulo.ForeColor = Ui.ColorPrimario;
        _titulo.TextAlign = ContentAlignment.MiddleLeft;
        _titulo.Padding = new Padding(16, 0, 0, 0);

        // Panel izquierdo con los campos
        GrupoDatos.Text = textoGrupo;
        GrupoDatos.Dock = DockStyle.Left;
        GrupoDatos.Width = 350;
        GrupoDatos.Padding = new Padding(6);

        // Botones dentro del panel izquierdo
        PanelBotones.Dock = DockStyle.Bottom;
        PanelBotones.Height = 104;
        BotonNuevo = Ui.Boton("Nuevo", Ui.ColorAcento, 150);
        BotonNuevo.Location = new Point(12, 10);
        BotonGuardar = Ui.Boton("Guardar", Ui.ColorExito, 150);
        BotonGuardar.Location = new Point(174, 10);
        BotonEliminar = Ui.Boton("Eliminar", Ui.ColorPeligro, 150);
        BotonEliminar.Location = new Point(12, 54);
        PanelBotones.Controls.Add(BotonNuevo);
        PanelBotones.Controls.Add(BotonGuardar);
        PanelBotones.Controls.Add(BotonEliminar);
        GrupoDatos.Controls.Add(PanelBotones);

        // Buscador
        PanelBusqueda.Dock = DockStyle.Top;
        PanelBusqueda.Height = 46;
        PanelBusqueda.Controls.Add(new Label { Text = "Buscar:", AutoSize = true, Left = 8, Top = 14 });
        CajaBusqueda.Left = 66;
        CajaBusqueda.Top = 10;
        CajaBusqueda.Width = 250;
        PanelBusqueda.Controls.Add(CajaBusqueda);

        // Tabla
        Ui.ConfigurarTabla(Tabla);
        Tabla.Dock = DockStyle.Fill;

        _panelCentro.Dock = DockStyle.Fill;
        _panelCentro.Padding = new Padding(8, 0, 8, 8);
        _panelCentro.Controls.Add(Tabla);          // Fill primero
        _panelCentro.Controls.Add(PanelBusqueda);  // luego el borde superior

        // El control que se agrega de último es el primero en acoplarse al borde.
        Controls.Add(_panelCentro);
        Controls.Add(GrupoDatos);
        Controls.Add(_titulo);

        // Eventos comunes
        BotonNuevo.Click += (s, e) => Nuevo();
        BotonGuardar.Click += (s, e) => Seguro(Guardar);
        BotonEliminar.Click += (s, e) => Seguro(Eliminar);
        CajaBusqueda.TextChanged += (s, e) => Seguro(Refrescar);
        Tabla.SelectionChanged += (s, e) => AlCambiarSeleccion();
        Load += (s, e) => Seguro(() =>
        {
            CargarInicial();
            Refrescar();
        });
    }

    // ---------- Lo que define cada módulo ----------

    /// <summary>Consulta los datos (con el texto del buscador) y los muestra llamando a Enlazar.</summary>
    protected abstract void CargarGrid(string filtro);

    /// <summary>Pasa los datos del registro seleccionado a los campos del formulario.</summary>
    protected abstract void MostrarSeleccion(object registro);

    /// <summary>Registra o actualiza según IdSeleccionado. Devuelve el mensaje de éxito.</summary>
    protected abstract string GuardarRegistro();

    /// <summary>Elimina el registro de IdSeleccionado. Devuelve el mensaje de éxito.</summary>
    protected abstract string EliminarRegistro();

    protected abstract void LimpiarCampos();

    protected virtual void CargarInicial()
    {
    }

    protected virtual void ConfigurarColumnas()
    {
    }

    protected virtual string PreguntaEliminar => "¿Está seguro de que desea eliminar el registro seleccionado?";

    // ---------- Utilidades para las clases hijas ----------

    /// <summary>Agrega una etiqueta y su control al panel de datos. 'fila' es la posición vertical (0, 1, 2...).</summary>
    protected T Campo<T>(string etiqueta, T control, int fila) where T : Control
    {
        int y = 24 + fila * 60;
        GrupoDatos.Controls.Add(new Label
        {
            Text = etiqueta,
            AutoSize = true,
            Left = 14,
            Top = y,
            ForeColor = Ui.ColorTexto
        });
        control.Left = 14;
        control.Top = y + 20;
        control.Width = 318;
        GrupoDatos.Controls.Add(control);
        return control;
    }

    /// <summary>Agrega un botón extra en la segunda fila de botones (junto a Eliminar).</summary>
    protected Button AgregarBotonExtra(string texto, Color color, EventHandler alHacerClic)
    {
        var boton = Ui.Boton(texto, color, 150);
        boton.Location = new Point(174, 54);
        boton.Click += alHacerClic;
        PanelBotones.Controls.Add(boton);
        return boton;
    }

    protected void Refrescar()
    {
        CargarGrid(CajaBusqueda.Text.Trim());
    }

    /// <summary>Muestra una lista de objetos en la tabla, conservando seleccionado el registro en edición.</summary>
    protected void Enlazar<T>(List<T> datos)
    {
        _cargando = true;
        try
        {
            Tabla.DataSource = null;
            Tabla.DataSource = datos;
            Ocultar("EsNuevo", "NombreCompleto", "Descripcion", "HayDisponibilidad", "Prestados", "EstaActivo");
            ConfigurarColumnas();
            Ui.FormatearFechas(Tabla);
            Tabla.ClearSelection();

            if (IdSeleccionado.HasValue)
            {
                foreach (DataGridViewRow fila in Tabla.Rows)
                {
                    if (fila.DataBoundItem is EntidadBase entidad && entidad.Id == IdSeleccionado.Value)
                    {
                        fila.Selected = true;
                        break;
                    }
                }
            }
        }
        finally
        {
            _cargando = false;
        }
    }

    protected void Encabezado(string propiedad, string texto)
    {
        if (Tabla.Columns.Contains(propiedad))
        {
            Tabla.Columns[propiedad].HeaderText = texto;
        }
    }

    protected void Ocultar(params string[] propiedades)
    {
        foreach (string propiedad in propiedades)
        {
            if (Tabla.Columns.Contains(propiedad))
            {
                Tabla.Columns[propiedad].Visible = false;
            }
        }
    }

    protected void Ordenar(params string[] propiedades)
    {
        int posicion = 0;
        foreach (string propiedad in propiedades)
        {
            if (Tabla.Columns.Contains(propiedad))
            {
                Tabla.Columns[propiedad].DisplayIndex = posicion++;
            }
        }
    }

    // ---------- Comportamiento común ----------

    protected void Nuevo()
    {
        _cargando = true;
        try
        {
            Tabla.ClearSelection();
        }
        finally
        {
            _cargando = false;
        }

        IdSeleccionado = null;
        LimpiarCampos();
    }

    private void AlCambiarSeleccion()
    {
        if (_cargando || Tabla.SelectedRows.Count == 0)
        {
            return;
        }

        object registro = Tabla.SelectedRows[0].DataBoundItem;
        if (registro == null)
        {
            return;
        }

        if (registro is EntidadBase entidad)
        {
            IdSeleccionado = entidad.Id;
        }

        MostrarSeleccion(registro);
    }

    private void Guardar()
    {
        string mensaje = GuardarRegistro();
        Ui.Info(mensaje);
        Refrescar();
        Nuevo();
    }

    private void Eliminar()
    {
        if (IdSeleccionado == null)
        {
            Ui.Advertencia("Seleccione primero un registro de la tabla.");
            return;
        }

        if (!Ui.Confirmar(PreguntaEliminar))
        {
            return;
        }

        string mensaje = EliminarRegistro();
        Ui.Info(mensaje);
        Refrescar();
        Nuevo();
    }
}
