namespace InclusiON.Application.UseCases.Persons.Queries
{
    public record GetPersonsQuery(
        int Page,
        int PageSize,
        string? Search,
        int? DisabilityTypeId,
        int? AutonomyLevelId,
        bool? IsActive,
        string? SortBy,
        string SortDirection
    );
}
