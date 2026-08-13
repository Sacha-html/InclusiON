using InclusiON.DTOs.Common;

namespace InclusiON.Application.UseCases.Reports.Queries
{
    public record GetReportsQuery(
        int Page,
        int PageSize,
        string? Search,
        string? PersonId,
        string? ProfessionalId,
        string? ReportTypeId,
        bool? IsActive,
        string? Status,
        DateTime? DateFrom,
        DateTime? DateTo,
        SortField? SortBy,
        string SortDirection,
        List<int>? InstitutionIds = null,
        List<string>? PersonIds = null
    );
}
