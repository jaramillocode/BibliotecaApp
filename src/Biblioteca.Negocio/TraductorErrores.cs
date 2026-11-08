using Biblioteca.Datos;

namespace Biblioteca.Negocio;

/// <summary>
/// Convierte los errores técnicos de la capa de datos en mensajes entendibles para el usuario.
/// </summary>
internal static class TraductorErrores
{
    /// <summary>Errores al insertar o actualizar.</summary>
    public static ReglaNegocioException AlGuardar(DatosException ex, string mensajeDuplicado)
    {
        switch (ex.Tipo)
        {
            case TipoErrorDatos.Duplicado:
                return new ReglaNegocioException(mensajeDuplicado, ex);
            case TipoErrorDatos.RestriccionIntegridad:
                return new ReglaNegocioException(
                    "Los datos no cumplen las restricciones de la base de datos. Revise las referencias y los valores ingresados.", ex);
            default:
                return new ReglaNegocioException(ex.Message, ex);
        }
    }

    /// <summary>Errores al eliminar.</summary>
    public static ReglaNegocioException AlEliminar(DatosException ex, string mensajeEnUso)
    {
        switch (ex.Tipo)
        {
            case TipoErrorDatos.RestriccionIntegridad:
                return new ReglaNegocioException(mensajeEnUso, ex);
            default:
                return new ReglaNegocioException(ex.Message, ex);
        }
    }

    /// <summary>Cualquier otro error de datos (consultas).</summary>
    public static ReglaNegocioException General(DatosException ex)
    {
        return new ReglaNegocioException(ex.Message, ex);
    }
}
