namespace InclusiON.Application.UseCases.Family.Queries
{
    public class GetPersonLinkHistoryQuery
    {
        public Guid PersonId { get; }

        public GetPersonLinkHistoryQuery(Guid personId)
        {
            PersonId = personId;
        }
    }
}
