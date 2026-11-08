using Biblioteca.Presentacion.Formularios;

namespace Biblioteca.Presentacion;

/// <summary>
/// Formulario principal: menú lateral para navegar entre los módulos.
/// Cada módulo se abre dentro del panel de contenido (no en ventanas separadas).
/// </summary>
public class FrmPrincipal : Form
{
    private readonly Panel _menu = new();
    private readonly Panel _contenido = new();
    private readonly List<Button> _botonesMenu = new();
    private Button _botonActivo;

    public FrmPrincipal()
    {
        Text = "Sistema de Gestión de Biblioteca";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(1280, 760);
        MinimumSize = new Size(1100, 640);
        Font = new Font("Segoe UI", 9.5f);
        BackColor = Ui.ColorFondo;

        _menu.Dock = DockStyle.Left;
        _menu.Width = 210;
        _menu.BackColor = Ui.ColorPrimario;

        _contenido.Dock = DockStyle.Fill;

        // Los botones con Dock = Top se apilan en orden inverso al de inserción,
        // por eso se agregan de abajo hacia arriba.
        var salir = CrearBotonMenu("Salir");
        salir.Dock = DockStyle.Bottom;
        salir.Click += (s, e) => Close();
        _menu.Controls.Add(salir);

        AgregarOpcion("Reportes", () => new FormReportes());
        AgregarOpcion("Préstamos", () => new FormPrestamos());
        AgregarOpcion("Editoriales", () => new FormEditoriales());
        AgregarOpcion("Autores", () => new FormAutores());
        AgregarOpcion("Usuarios", () => new FormUsuarios());
        AgregarOpcion("Libros", () => new FormLibros());
        AgregarOpcion("Inicio", () => new FormInicio());

        _menu.Controls.Add(new Label
        {
            Text = "BIBLIOTECA",
            Dock = DockStyle.Top,
            Height = 80,
            ForeColor = Color.White,
            Font = new Font("Segoe UI Semibold", 15f),
            TextAlign = ContentAlignment.MiddleCenter
        });

        Controls.Add(_contenido);
        Controls.Add(_menu);

        // Pantalla de inicio al abrir la aplicación.
        Load += (s, e) => Abrir(_botonesMenu.First(b => b.Text.Trim() == "Inicio"), () => new FormInicio());
    }

    private Button CrearBotonMenu(string texto)
    {
        var boton = new Button
        {
            Text = "   " + texto,
            Height = 48,
            Dock = DockStyle.Top,
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.White,
            BackColor = Ui.ColorPrimario,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 11f),
            Cursor = Cursors.Hand
        };
        boton.FlatAppearance.BorderSize = 0;
        boton.FlatAppearance.MouseOverBackColor = Ui.ColorAcento;
        return boton;
    }

    private void AgregarOpcion(string texto, Func<FormEmbebido> crearFormulario)
    {
        Button boton = CrearBotonMenu(texto);
        boton.Click += (s, e) => Abrir(boton, crearFormulario);
        _botonesMenu.Add(boton);
        _menu.Controls.Add(boton);
    }

    private void Abrir(Button boton, Func<FormEmbebido> crearFormulario)
    {
        try
        {
            // Cierra el módulo anterior para liberar sus recursos.
            foreach (Form anterior in _contenido.Controls.OfType<Form>().ToList())
            {
                _contenido.Controls.Remove(anterior);
                anterior.Dispose();
            }

            FormEmbebido formulario = crearFormulario();
            _contenido.Controls.Add(formulario);
            formulario.Show();
            Resaltar(boton);
        }
        catch (Exception ex)
        {
            Ui.Error("No se pudo abrir el módulo: " + ex.Message);
        }
    }

    private void Resaltar(Button boton)
    {
        if (_botonActivo != null)
        {
            _botonActivo.BackColor = Ui.ColorPrimario;
        }

        _botonActivo = boton;
        _botonActivo.BackColor = Ui.ColorAcento;
    }
}
