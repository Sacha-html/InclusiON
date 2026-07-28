using InclusiON.DTOs.Common;

namespace InclusiON.Application.UseCases.AdminUsers.Queries
{
    public record GetAdminUsersQuery(
        int Page,
        int PageSize,
        string? Search,
        string? Role,
        bool? IsActive,
        SortField? SortBy,
        string SortDirection,
        List<int>? InstitutionIds = null
    );
}
