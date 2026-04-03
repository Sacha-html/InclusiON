using InclusiON.DTOs.Common;

namespace InclusiON.Application.UseCases.Professionals.Queries
{
    public record GetProfessionalsQuery(
        int Page,
        int PageSize,
        string? Search,
        string? Specialty,
        bool? IsActive,
        string? Status,
        SortField? SortBy,
        string SortDirection,
        List<int>? InstitutionIds = null
    );
}
