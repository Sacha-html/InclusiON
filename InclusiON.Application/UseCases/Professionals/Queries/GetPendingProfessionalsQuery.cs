using InclusiON.DTOs.Common;

namespace InclusiON.Application.UseCases.Professionals.Queries
{
    public record GetPendingProfessionalsQuery(
        int Page,
        int PageSize,
        string? Search,
        SortField? SortBy,
        string SortDirection
    );
}