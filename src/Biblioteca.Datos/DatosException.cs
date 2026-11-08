namespace Biblioteca.Datos;

/// <summary>Clasificación de los errores de base de datos que le interesan a la capa de negocio.</summary>
public enum TipoErrorDatos
{
    Desconocido,
    Conexion,
    Duplicado,
    RestriccionIntegridad,
    SinDisponibilidad,
    NoEncontrado
}

/// <summary>
/// Excepción propia de la capa de datos. Así las capas superiores no dependen de SqlException
/// ni de los números de error de SQL Server.
/// </summary>
public class DatosException : Exception
{
    public TipoErrorDatos Tipo { get; }

    public DatosException(TipoErrorDatos tipo, string mensaje, Exception interna = null)
        : base(mensaje, interna)
    {
        Tipo = tipo;
    }
}
