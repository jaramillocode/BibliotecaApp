using System.Data;
using Microsoft.Data.SqlClient;

namespace Biblioteca.Datos;

/// <summary>
/// Ejecuta comandos SQL parametrizados y traduce los errores de SQL Server a DatosException.
/// Todas las clases de acceso a datos pasan por aquí, de modo que ninguna consulta se arma
/// concatenando texto del usuario (evita inyección SQL).
/// </summary>
internal static class Ejecutor
{
    public static List<T> Consultar<T>(string sql, Func<SqlDataReader, T> mapear, params SqlParameter[] parametros)
    {
        try
        {
            using var conexion = ConexionBD.CrearConexion();
            using var comando = new SqlCommand(sql, conexion);
            comando.Parameters.AddRange(parametros);
            conexion.Open();
            using var lector = comando.ExecuteReader();
            var lista = new List<T>();
            while (lector.Read())
            {
                lista.Add(mapear(lector));
            }

            return lista;
        }
        catch (SqlException ex)
        {
            throw Traducir(ex);
        }
    }

    public static int NoQuery(string sql, params SqlParameter[] parametros)
    {
        try
        {
            using var conexion = ConexionBD.CrearConexion();
            using var comando = new SqlCommand(sql, conexion);
            comando.Parameters.AddRange(parametros);
            conexion.Open();
            return comando.ExecuteNonQuery();
        }
        catch (SqlException ex)
        {
            throw Traducir(ex);
        }
    }

    public static object Escalar(string sql, params SqlParameter[] parametros)
    {
        try
        {
            using var conexion = ConexionBD.CrearConexion();
            using var comando = new SqlCommand(sql, conexion);
            comando.Parameters.AddRange(parametros);
            conexion.Open();
            return comando.ExecuteScalar();
        }
        catch (SqlException ex)
        {
            throw Traducir(ex);
        }
    }

    public static DataTable Tabla(string sql, params SqlParameter[] parametros)
    {
        try
        {
            using var conexion = ConexionBD.CrearConexion();
            using var comando = new SqlCommand(sql, conexion);
            comando.Parameters.AddRange(parametros);
            using var adaptador = new SqlDataAdapter(comando);
            var tabla = new DataTable();
            adaptador.Fill(tabla);
            return tabla;
        }
        catch (SqlException ex)
        {
            throw Traducir(ex);
        }
    }

    // ---------- Fábricas de parámetros ----------

    public static SqlParameter P(string nombre, object valor)
    {
        return new SqlParameter(nombre, valor ?? DBNull.Value);
    }

    public static SqlParameter PTexto(string nombre, string valor)
    {
        var parametro = new SqlParameter(nombre, SqlDbType.NVarChar);
        parametro.Value = string.IsNullOrWhiteSpace(valor) ? DBNull.Value : valor.Trim();
        return parametro;
    }

    public static SqlParameter PFecha(string nombre, DateTime? valor)
    {
        var parametro = new SqlParameter(nombre, SqlDbType.Date);
        parametro.Value = valor.HasValue ? valor.Value.Date : DBNull.Value;
        return parametro;
    }

    // ---------- Traducción de errores ----------

    public static DatosException Traducir(SqlException ex)
    {
        switch (ex.Number)
        {
            case 2601: // índice único violado
            case 2627: // restricción UNIQUE/PRIMARY KEY violada
                return new DatosException(TipoErrorDatos.Duplicado,
                    "Ya existe un registro con esos datos.", ex);

            case 547: // conflicto con FOREIGN KEY o CHECK
                return new DatosException(TipoErrorDatos.RestriccionIntegridad,
                    "La operación viola una restricción de integridad de la base de datos.", ex);

            case -1:
            case 2:
            case 53:
            case 233:
            case 4060:
            case 18456:
                return new DatosException(TipoErrorDatos.Conexion,
                    "No se pudo conectar con SQL Server. Verifique que el servicio esté en ejecución, " +
                    "que la base de datos BibliotecaDB exista y que la cadena de conexión de App.config sea correcta.", ex);

            default:
                return new DatosException(TipoErrorDatos.Desconocido,
                    "Error en la base de datos: " + ex.Message, ex);
        }
    }
}

/// <summary>Métodos de ayuda para leer columnas por nombre, tolerando valores NULL.</summary>
internal static class ExtensionesLector
{
    /// <summary>
    /// Lee un número entero sin importar si la columna es SMALLINT o INT
    /// (GetInt32 falla con SMALLINT, como la columna AnioPublicacion).
    /// </summary>
    public static int Entero(this SqlDataReader lector, string columna)
    {
        return Convert.ToInt32(lector.GetValue(lector.GetOrdinal(columna)));
    }

    public static string Texto(this SqlDataReader lector, string columna)
    {
        int indice = lector.GetOrdinal(columna);
        return lector.IsDBNull(indice) ? null : lector.GetString(indice);
    }

    public static DateTime Fecha(this SqlDataReader lector, string columna)
    {
        return lector.GetDateTime(lector.GetOrdinal(columna));
    }

    public static DateTime? FechaNula(this SqlDataReader lector, string columna)
    {
        int indice = lector.GetOrdinal(columna);
        return lector.IsDBNull(indice) ? null : lector.GetDateTime(indice);
    }
}
