using InclusiON.DTOs.Common;

namespace InclusiON.Application.UseCases.Family.Queries
{
    public record GetFamilyQuery(
        int Page, int PageSize, string? Search, bool? IsActive,
        SortField? SortBy, string SortDirection,
        int? InstitutionId = null
    );
}
