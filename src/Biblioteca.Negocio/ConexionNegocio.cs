using Biblioteca.Datos;

namespace Biblioteca.Negocio;

/// <summary>Permite a la interfaz verificar la conexión sin depender directamente de la capa de datos.</summary>
public static class ConexionNegocio
{
    public static bool Probar(out string mensaje) => ConexionBD.Probar(out mensaje);
}
