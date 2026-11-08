using System.Text.RegularExpressions;

namespace Biblioteca.Negocio;

/// <summary>Validaciones reutilizables para los datos que escribe el usuario.</summary>
internal static class Validador
{
    private static readonly Regex PatronCorreo =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    private static readonly Regex PatronTelefono =
        new(@"^[0-9+\-\s]{7,20}$", RegexOptions.Compiled);

    private static readonly Regex PatronDocumento =
        new(@"^[A-Za-z0-9]{5,20}$", RegexOptions.Compiled);

    private static readonly Regex PatronIsbn =
        new(@"^[0-9Xx\-]{10,17}$", RegexOptions.Compiled);

    /// <summary>Texto obligatorio: no vacío y dentro de la longitud permitida. Devuelve el texto sin espacios sobrantes.</summary>
    public static string Requerido(string valor, string campo, int maximo)
    {
        string limpio = (valor ?? string.Empty).Trim();
        if (limpio.Length == 0)
        {
            throw new ReglaNegocioException($"{campo} es obligatorio.");
        }

        if (limpio.Length > maximo)
        {
            throw new ReglaNegocioException($"{campo} no puede superar los {maximo} caracteres.");
        }

        return limpio;
    }

    /// <summary>Texto opcional: si viene vacío devuelve null; si trae algo, respeta la longitud máxima.</summary>
    public static string Opcional(string valor, string campo, int maximo)
    {
        string limpio = (valor ?? string.Empty).Trim();
        if (limpio.Length == 0)
        {
            return null;
        }

        if (limpio.Length > maximo)
        {
            throw new ReglaNegocioException($"{campo} no puede superar los {maximo} caracteres.");
        }

        return limpio;
    }

    public static string Correo(string valor)
    {
        string limpio = Opcional(valor, "El correo electrónico", 120);
        if (limpio != null && !PatronCorreo.IsMatch(limpio))
        {
            throw new ReglaNegocioException("El correo electrónico no tiene un formato válido (ejemplo: nombre@dominio.com).");
        }

        return limpio;
    }

    public static string Telefono(string valor)
    {
        string limpio = Opcional(valor, "El teléfono", 20);
        if (limpio != null && !PatronTelefono.IsMatch(limpio))
        {
            throw new ReglaNegocioException("El teléfono solo puede tener números, espacios, + y guiones (entre 7 y 20 caracteres).");
        }

        return limpio;
    }

    public static string Documento(string valor)
    {
        string limpio = Requerido(valor, "El documento", 20);
        if (!PatronDocumento.IsMatch(limpio))
        {
            throw new ReglaNegocioException("El documento solo puede tener letras y números, sin espacios (entre 5 y 20 caracteres).");
        }

        return limpio;
    }

    public static string Isbn(string valor)
    {
        string limpio = Requerido(valor, "El código ISBN", 17);
        if (!PatronIsbn.IsMatch(limpio))
        {
            throw new ReglaNegocioException("El ISBN solo puede tener números, guiones y la letra X (entre 10 y 17 caracteres).");
        }

        return limpio.ToUpperInvariant();
    }
}
