using System.Data;
using System.Text;
using Biblioteca.Negocio;

namespace Biblioteca.Presentacion.Formularios;

/// <summary>Módulo de reportes: el usuario elige una consulta y ve el resultado en una tabla.</summary>
public class FormReportes : FormEmbebido
{
    private readonly ReporteNegocio _negocio = new();
    private readonly ComboBox _cboReporte = new();
    private readonly Button _botonGenerar;
    private readonly Button _botonExportar;
    private readonly Label _lblResultado = new();
    private readonly DataGridView _tabla = new();
    private DataTable _tablaActual;

    public FormReportes()
    {
        var titulo = new Label
        {
            Text = "Consultas y reportes",
            Dock = DockStyle.Top,
            Height = 56,
            Font = new Font("Segoe UI Semibold", 16f),
            ForeColor = Ui.ColorPrimario,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(16, 0, 0, 0)
        };

        var barra = new Panel { Dock = DockStyle.Top, Height = 52 };
        barra.Controls.Add(new Label { Text = "Reporte:", AutoSize = true, Left = 16, Top = 17 });

        _cboReporte.DropDownStyle = ComboBoxStyle.DropDownList;
        _cboReporte.Left = 80;
        _cboReporte.Top = 13;
        _cboReporte.Width = 360;
        barra.Controls.Add(_cboReporte);

        _botonGenerar = Ui.Boton("Generar", Ui.ColorAcento, 120);
        _botonGenerar.Location = new Point(456, 8);
        barra.Controls.Add(_botonGenerar);

        _botonExportar = Ui.Boton("Exportar a CSV", Ui.ColorExito, 140);
        _botonExportar.Location = new Point(588, 8);
        barra.Controls.Add(_botonExportar);

        _lblResultado.Dock = DockStyle.Bottom;
        _lblResultado.Height = 30;
        _lblResultado.Padding = new Padding(16, 6, 0, 0);
        _lblResultado.ForeColor = Ui.ColorTexto;
        _lblResultado.Text = "Seleccione un reporte y presione Generar.";

        Ui.ConfigurarTabla(_tabla);
        _tabla.Dock = DockStyle.Fill;

        var contenedor = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16, 0, 16, 0) };
        contenedor.Controls.Add(_tabla);

        Controls.Add(contenedor);
        Controls.Add(_lblResultado);
        Controls.Add(barra);
        Controls.Add(titulo);

        _botonGenerar.Click += (s, e) => Seguro(Generar);
        _botonExportar.Click += (s, e) => Seguro(ExportarCsv);
        _cboReporte.SelectedIndexChanged += (s, e) =>
        {
            if (IsHandleCreated)
            {
                Seguro(Generar);
            }
        };

        Load += (s, e) => Seguro(() =>
        {
            foreach (string nombre in _negocio.NombresReportes)
            {
                _cboReporte.Items.Add(nombre);
            }

            _cboReporte.SelectedIndex = 0; // genera el primer reporte (resumen general)
        });
    }

    private void Generar()
    {
        if (_cboReporte.SelectedItem == null)
        {
            Ui.Advertencia("Seleccione un reporte de la lista.");
            return;
        }

        _tablaActual = _negocio.Generar(_cboReporte.SelectedItem.ToString());
        _tabla.DataSource = _tablaActual;
        Ui.FormatearFechas(_tabla);
        _tabla.ClearSelection();
        _lblResultado.Text = _tablaActual.Rows.Count == 1
            ? "1 registro encontrado."
            : $"{_tablaActual.Rows.Count} registros encontrados.";
    }

    private void ExportarCsv()
    {
        if (_tablaActual == null || _tablaActual.Rows.Count == 0)
        {
            Ui.Advertencia("No hay datos para exportar. Genere primero un reporte.");
            return;
        }

        using var dialogo = new SaveFileDialog
        {
            Filter = "Archivo CSV (*.csv)|*.csv",
            FileName = "reporte_biblioteca.csv"
        };
        if (dialogo.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        // Punto y coma como separador: es el que espera Excel con configuración regional de Colombia.
        var texto = new StringBuilder();
        texto.AppendLine(string.Join(";", _tablaActual.Columns.Cast<DataColumn>().Select(c => Escapar(c.ColumnName))));
        foreach (DataRow fila in _tablaActual.Rows)
        {
            texto.AppendLine(string.Join(";", fila.ItemArray.Select(Formatear)));
        }

        File.WriteAllText(dialogo.FileName, texto.ToString(), new UTF8Encoding(true));
        Ui.Info("El reporte se exportó correctamente.");
    }

    private static string Formatear(object valor)
    {
        if (valor == null || valor == DBNull.Value)
        {
            return string.Empty;
        }

        if (valor is DateTime fecha)
        {
            return fecha.ToString("dd/MM/yyyy");
        }

        return Escapar(valor.ToString());
    }

    private static string Escapar(string texto)
    {
        return "\"" + texto.Replace("\"", "\"\"") + "\"";
    }
}
