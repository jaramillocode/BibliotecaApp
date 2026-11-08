namespace Biblioteca.Negocio;

/// <summary>
/// Se lanza cuando un dato no es válido o una operación incumple una regla del sistema.
/// El mensaje está redactado para mostrarse tal cual al usuario.
/// </summary>
public class ReglaNegocioException : Exception
{
    public ReglaNegocioException(string mensaje) : base(mensaje)
    {
    }

    public ReglaNegocioException(string mensaje, Exception interna) : base(mensaje, interna)
    {
    }
}
