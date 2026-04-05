namespace InclusiON.Application.UseCases.Family.Queries
{
    public class GetFamilyLinkHistoryQuery
    {
        public Guid FamilyId { get; }

        public GetFamilyLinkHistoryQuery(Guid familyId)
        {
            FamilyId = familyId;
        }
    }
}
