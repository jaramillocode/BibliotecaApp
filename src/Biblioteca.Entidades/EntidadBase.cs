namespace Biblioteca.Entidades;

/// <summary>
/// Clase base de todas las entidades del sistema. Concentra el identificador
/// (clave primaria) para no repetirlo en cada clase.
/// </summary>
public abstract class EntidadBase
{
    public int Id { get; set; }

    protected EntidadBase()
    {
    }

    protected EntidadBase(int id)
    {
        Id = id;
    }

    /// <summary>Indica si el objeto ya fue guardado en la base de datos.</summary>
    public bool EsNuevo => Id <= 0;
}
