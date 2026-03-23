using InclusiON.DTOs.Common;

namespace InclusiON.Application.UseCases.Professionals.Queries
{
    public record GetProfessionalsQuery(
        int Page,
        int PageSize,
        string? Search,
        string? Specialty,
        bool? IsActive,
        SortField? SortBy,
        string SortDirection,
        int? InstitutionId = null
    );
}
