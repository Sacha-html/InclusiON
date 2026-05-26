namespace InclusiON.Application.UseCases.AdminInstitutions.Queries
{
    public record GetAllAdminsQuery(
        int Page = 1,
        int PageSize = 10,
        string? Search = null
    );
}
