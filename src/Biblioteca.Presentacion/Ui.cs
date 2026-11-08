namespace Biblioteca.Presentacion;

/// <summary>Colores y utilidades visuales compartidas por todos los formularios.</summary>
internal static class Ui
{
    public static readonly Color ColorPrimario = Color.FromArgb(31, 78, 121);
    public static readonly Color ColorAcento = Color.FromArgb(46, 117, 182);
    public static readonly Color ColorExito = Color.FromArgb(39, 134, 90);
    public static readonly Color ColorPeligro = Color.FromArgb(192, 57, 43);
    public static readonly Color ColorFondo = Color.FromArgb(244, 246, 249);
    public static readonly Color ColorTexto = Color.FromArgb(50, 50, 50);

    private const string Titulo = "Sistema de Gestión de Biblioteca";

    public static Button Boton(string texto, Color color, int ancho)
    {
        var boton = new Button
        {
            Text = texto,
            Width = ancho,
            Height = 36,
            BackColor = color,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        boton.FlatAppearance.BorderSize = 0;
        return boton;
    }

    public static void ConfigurarTabla(DataGridView tabla)
    {
        tabla.ReadOnly = true;
        tabla.AllowUserToAddRows = false;
        tabla.AllowUserToDeleteRows = false;
        tabla.AllowUserToResizeRows = false;
        tabla.MultiSelect = false;
        tabla.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        tabla.RowHeadersVisible = false;
        tabla.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        tabla.BackgroundColor = Color.White;
        tabla.BorderStyle = BorderStyle.None;
        tabla.EnableHeadersVisualStyles = false;
        tabla.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        tabla.ColumnHeadersHeight = 36;
        tabla.ColumnHeadersDefaultCellStyle.BackColor = ColorPrimario;
        tabla.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        tabla.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9.5f);
        tabla.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(238, 243, 249);
        tabla.DefaultCellStyle.SelectionBackColor = ColorAcento;
        tabla.DefaultCellStyle.SelectionForeColor = Color.White;
        tabla.RowTemplate.Height = 28;
    }

    /// <summary>Muestra las fechas como dd/MM/yyyy en todas las columnas de tipo fecha.</summary>
    public static void FormatearFechas(DataGridView tabla)
    {
        foreach (DataGridViewColumn columna in tabla.Columns)
        {
            if (columna.ValueType == typeof(DateTime) || columna.ValueType == typeof(DateTime?))
            {
                columna.DefaultCellStyle.Format = "dd/MM/yyyy";
            }
        }
    }

    /// <summary>Asigna una fecha a un DateTimePicker sin salirse de su rango permitido.</summary>
    public static void PonerFecha(DateTimePicker selector, DateTime fecha)
    {
        if (fecha < selector.MinDate)
        {
            fecha = selector.MinDate;
        }

        if (fecha > selector.MaxDate)
        {
            fecha = selector.MaxDate;
        }

        selector.Value = fecha;
    }

    public static void Info(string mensaje) =>
        MessageBox.Show(mensaje, Titulo, MessageBoxButtons.OK, MessageBoxIcon.Information);

    public static void Advertencia(string mensaje) =>
        MessageBox.Show(mensaje, Titulo, MessageBoxButtons.OK, MessageBoxIcon.Warning);

    public static void Error(string mensaje) =>
        MessageBox.Show(mensaje, Titulo, MessageBoxButtons.OK, MessageBoxIcon.Error);

    public static bool Confirmar(string pregunta) =>
        MessageBox.Show(pregunta, Titulo, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
}
