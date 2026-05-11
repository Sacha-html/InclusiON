namespace InclusiON.Domain.Models.BaseEntities
{
    /// <summary>
    /// Marca entidades con clave primaria entera.
    /// Permite consultas genéricas por Id sin depender de FindAsync (que no soporta AsNoTracking).
    /// </summary>
    public interface IHasIntId
    {
        int Id { get; }
    }
}
