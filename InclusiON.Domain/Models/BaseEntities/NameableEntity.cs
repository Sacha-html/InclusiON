namespace InclusiON.Domain.Models.BaseEntities
{
    public class NameableEntity: IdentifiableEntity
    {
        public string Name { get; set; } = string.Empty;
    }
}
