namespace InclusiON.Application.UseCases.Family.Queries
{
    public class GetFamilyStatusHistoryQuery
    {
        public Guid FamilyId { get; }

        public GetFamilyStatusHistoryQuery(Guid familyId)
        {
            FamilyId = familyId;
        }
    }
}
