namespace InclusiON.Application.UseCases.Family.Queries
{
    public class GetAvailableFamiliesQuery
    {
        public string? Search { get; }
        public Guid? PersonId { get; }

        public GetAvailableFamiliesQuery(string? search, Guid? personId = null)
        {
            Search = search;
            PersonId = personId;
        }
    }
}
