using Biblioteca.Negocio;

namespace Biblioteca.Presentacion.Formularios;

/// <summary>Pantalla de bienvenida con las cifras principales de la biblioteca.</summary>
public class FormInicio : FormEmbebido
{
    private readonly ReporteNegocio _negocio = new();
    private readonly FlowLayoutPanel _tarjetas = new();

    public FormInicio()
    {
        var encabezado = new Panel { Dock = DockStyle.Top, Height = 130, BackColor = Ui.ColorPrimario };
        encabezado.Controls.Add(new Label
        {
            Text = "Sistema de Gestión de Biblioteca",
            AutoSize = true,
            Left = 32,
            Top = 28,
            ForeColor = Color.White,
            Font = new Font("Segoe UI Semibold", 22f)
        });
        encabezado.Controls.Add(new Label
        {
            Text = "Administre libros, autores, editoriales, usuarios, préstamos y devoluciones desde un solo lugar.",
            AutoSize = true,
            Left = 34,
            Top = 78,
            ForeColor = Color.FromArgb(220, 230, 240),
            Font = new Font("Segoe UI", 11f)
        });

        _tarjetas.Dock = DockStyle.Top;
        _tarjetas.Height = 170;
        _tarjetas.Padding = new Padding(24, 24, 0, 0);

        var ayuda = new Label
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(32, 8, 32, 0),
            ForeColor = Ui.ColorTexto,
            Font = new Font("Segoe UI", 10.5f),
            Text = "Cómo empezar\r\n\r\n" +
                   "1. Registre los autores y las editoriales.\r\n" +
                   "2. Registre los libros eligiendo su autor y su editorial.\r\n" +
                   "3. Registre los usuarios de la biblioteca.\r\n" +
                   "4. En Préstamos, elija el usuario y el libro para prestarlo; cuando lo devuelvan, " +
                   "seleccione el préstamo y presione \"Registrar devolución\".\r\n" +
                   "5. En Reportes consulte el estado general y exporte los resultados a CSV."
        };

        Controls.Add(ayuda);
        Controls.Add(_tarjetas);
        Controls.Add(encabezado);

        Load += (s, e) => Seguro(CargarTarjetas);
    }

    private void CargarTarjetas()
    {
        var resumen = _negocio.ObtenerResumen();
        string[] mostrar =
        {
            "Títulos registrados", "Ejemplares disponibles", "Autores",
            "Editoriales", "Usuarios", "Préstamos activos"
        };

        _tarjetas.Controls.Clear();
        foreach (string concepto in mostrar)
        {
            int valor = resumen.TryGetValue(concepto, out int v) ? v : 0;
            _tarjetas.Controls.Add(CrearTarjeta(concepto, valor));
        }
    }

    private static Panel CrearTarjeta(string concepto, int valor)
    {
        var tarjeta = new Panel
        {
            Width = 170,
            Height = 110,
            BackColor = Color.White,
            Margin = new Padding(0, 0, 16, 0)
        };
        tarjeta.Controls.Add(new Label
        {
            Text = valor.ToString(),
            AutoSize = true,
            Left = 14,
            Top = 14,
            ForeColor = Ui.ColorAcento,
            Font = new Font("Segoe UI Semibold", 26f)
        });
        tarjeta.Controls.Add(new Label
        {
            Text = concepto,
            AutoSize = true,
            Left = 16,
            Top = 74,
            ForeColor = Ui.ColorTexto,
            Font = new Font("Segoe UI", 10f)
        });
        return tarjeta;
    }
}
