using System.Configuration;
using Microsoft.Data.SqlClient;

namespace Biblioteca.Datos;

/// <summary>
/// Administra la conexión con SQL Server. La cadena de conexión se lee de App.config
/// (entrada "BibliotecaDB") para poder cambiarla sin recompilar.
/// </summary>
public static class ConexionBD
{
    private const string NombreCadena = "BibliotecaDB";

    public static string CadenaConexion
    {
        get
        {
            var configuracion = ConfigurationManager.ConnectionStrings[NombreCadena];
            if (configuracion == null || string.IsNullOrWhiteSpace(configuracion.ConnectionString))
            {
                throw new DatosException(TipoErrorDatos.Conexion,
                    $"No se encontró la cadena de conexión '{NombreCadena}' en el archivo App.config.");
            }

            return configuracion.ConnectionString;
        }
    }

    public static SqlConnection CrearConexion() => new SqlConnection(CadenaConexion);

    /// <summary>Intenta abrir y cerrar una conexión para verificar que SQL Server responde.</summary>
    public static bool Probar(out string mensaje)
    {
        try
        {
            using var conexion = CrearConexion();
            conexion.Open();
            mensaje = "Conexión exitosa.";
            return true;
        }
        catch (DatosException ex)
        {
            mensaje = ex.Message;
            return false;
        }
        catch (SqlException ex)
        {
            mensaje = ex.Message;
            return false;
        }
        catch (Exception ex)
        {
            mensaje = ex.Message;
            return false;
        }
    }
}
