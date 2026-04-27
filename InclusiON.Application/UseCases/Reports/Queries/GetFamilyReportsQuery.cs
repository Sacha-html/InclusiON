using InclusiON.DTOs.Common;

namespace InclusiON.Application.UseCases.Reports.Queries
{
    public record GetFamilyReportsQuery(
        Guid FamilyRepresentativeId,
        int Page,
        int PageSize,
        string? ReportTypeId,
        DateTime? DateFrom,
        DateTime? DateTo,
        SortField? SortBy,
        string SortDirection
    );
}
