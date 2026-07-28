namespace InclusiON.Application.UseCases.Institutions.Queries
{
    public record GetInstitutionsQuery(
        int Page = 1,
        int PageSize = 10,
        string? Search = null,
        bool? IsActive = null
    );
}
