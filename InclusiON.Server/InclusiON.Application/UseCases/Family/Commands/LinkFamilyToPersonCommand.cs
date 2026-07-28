namespace InclusiON.Application.UseCases.Family.Commands
{
    public class LinkFamilyToPersonCommand
    {
        public Guid FamilyId { get; set; }
        public Guid PersonId { get; set; }
        public string Relationship { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public Guid ChangedByUserId { get; set; }

        public LinkFamilyToPersonCommand(Guid familyId, Guid personId, string relationship, bool isPrimary, Guid changedByUserId)
        {
            FamilyId = familyId;
            PersonId = personId;
            Relationship = relationship;
            IsPrimary = isPrimary;
            ChangedByUserId = changedByUserId;
        }
    }
}
