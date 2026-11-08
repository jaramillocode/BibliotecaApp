using Biblioteca.Negocio;

namespace Biblioteca.Presentacion.Formularios;

/// <summary>
/// Base de todas las pantallas que se muestran dentro del formulario principal.
/// Configura la ventana para que se acople al panel de contenido y centraliza el manejo de errores.
/// </summary>
public abstract class FormEmbebido : Form
{
    protected FormEmbebido()
    {
        TopLevel = false;
        FormBorderStyle = FormBorderStyle.None;
        Dock = DockStyle.Fill;
        BackColor = Ui.ColorFondo;
        Font = new Font("Segoe UI", 9.5f);
    }

    /// <summary>
    /// Ejecuta una acción de la interfaz y muestra al usuario un mensaje claro si algo falla:
    /// advertencia para errores de validación, error para los inesperados.
    /// </summary>
    protected void Seguro(Action accion)
    {
        try
        {
            accion();
        }
        catch (ReglaNegocioException ex)
        {
            Ui.Advertencia(ex.Message);
        }
        catch (Exception ex)
        {
            Ui.Error("Ocurrió un error inesperado: " + ex.Message);
        }
    }
}
