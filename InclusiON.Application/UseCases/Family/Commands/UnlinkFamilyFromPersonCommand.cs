namespace InclusiON.Application.UseCases.Family.Commands
{
    public class UnlinkFamilyFromPersonCommand
    {
        public Guid FamilyId { get; set; }
        public Guid PersonId { get; set; }
        public string Observation { get; set; } = string.Empty;
        public Guid ChangedByUserId { get; set; }

        public UnlinkFamilyFromPersonCommand(Guid familyId, Guid personId, string observation, Guid changedByUserId)
        {
            FamilyId = familyId;
            PersonId = personId;
            Observation = observation;
            ChangedByUserId = changedByUserId;
        }
    }
}
