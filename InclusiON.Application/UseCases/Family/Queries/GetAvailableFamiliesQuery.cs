namespace InclusiON.Application.UseCases.Family.Queries
{
    public class GetAvailableFamiliesQuery
    {
        public string? Search { get; }
        public Guid? PersonId { get; }
        public int Page { get; }
        public int PageSize { get; }

        public GetAvailableFamiliesQuery(string? search, Guid? personId = null, int page = 1, int pageSize = 50)
        {
            Search = search;
            PersonId = personId;
            Page = page;
            PageSize = pageSize;
        }
    }
}
