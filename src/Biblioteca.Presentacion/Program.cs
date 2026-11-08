using Biblioteca.Negocio;

namespace Biblioteca.Presentacion;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        // Antes de abrir la ventana principal se comprueba que SQL Server responda.
        if (!ConexionNegocio.Probar(out string mensaje))
        {
            MessageBox.Show(
                "No se pudo conectar con la base de datos.\n\n" + mensaje + "\n\n" +
                "Revise que:\n" +
                "1. SQL Server esté en ejecución.\n" +
                "2. Haya ejecutado el script database/biblioteca_script.sql.\n" +
                "3. La cadena de conexión en App.config tenga el nombre correcto de su servidor.",
                "Error de conexión",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        Application.Run(new FrmPrincipal());
    }
}
