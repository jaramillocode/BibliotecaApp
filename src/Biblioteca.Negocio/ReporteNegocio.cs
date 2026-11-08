using System.Data;
using Biblioteca.Datos;
using Biblioteca.Entidades;

namespace Biblioteca.Negocio;

/// <summary>Arma los reportes disponibles y los entrega listos para mostrarse en una tabla.</summary>
public class ReporteNegocio
{
    private readonly ReporteDatos _datos = new();
    private readonly Dictionary<string, Func<DataTable>> _reportes;

    public ReporteNegocio()
    {
        // El orden de inserción es el orden en que aparecen en la lista desplegable.
        _reportes = new Dictionary<string, Func<DataTable>>
        {
            ["Resumen general de la biblioteca"] = _datos.ResumenGeneral,
            ["Listado de libros registrados"] = _datos.ListadoLibros,
            ["Listado de autores"] = _datos.ListadoAutores,
            ["Listado de editoriales"] = _datos.ListadoEditoriales,
            ["Listado de usuarios registrados"] = _datos.ListadoUsuarios,
            ["Relación de préstamos registrados"] = _datos.RelacionPrestamos,
            ["Préstamos activos"] = () => _datos.PrestamosPorEstado(EstadoPrestamo.Prestado),
            ["Préstamos devueltos"] = () => _datos.PrestamosPorEstado(EstadoPrestamo.Devuelto),
            ["Historial de préstamos por usuario"] = _datos.HistorialPrestamos,
            ["Libros más prestados"] = _datos.LibrosMasPrestados
        };
    }

    public IReadOnlyList<string> NombresReportes => _reportes.Keys.ToList();

    public DataTable Generar(string nombreReporte)
    {
        if (!_reportes.TryGetValue(nombreReporte ?? string.Empty, out var generador))
        {
            throw new ReglaNegocioException("Seleccione un reporte de la lista.");
        }

        try { return generador(); }
        catch (DatosException ex) { throw TraductorErrores.General(ex); }
    }

    /// <summary>Cifras para las tarjetas de la pantalla de inicio.</summary>
    public Dictionary<string, int> ObtenerResumen()
    {
        try
        {
            var resumen = new Dictionary<string, int>();
            foreach (DataRow fila in _datos.ResumenGeneral().Rows)
            {
                resumen[(string)fila["Concepto"]] = (int)fila["Valor"];
            }

            return resumen;
        }
        catch (DatosException ex)
        {
            throw TraductorErrores.General(ex);
        }
    }
}
