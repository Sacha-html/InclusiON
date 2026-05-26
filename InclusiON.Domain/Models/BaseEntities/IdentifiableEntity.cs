namespace InclusiON.Domain.Models.BaseEntities
{
    public class IdentifiableEntity : BaseEntity, IHasIntId
    {
        public int Id { get; set; }
    }
}
