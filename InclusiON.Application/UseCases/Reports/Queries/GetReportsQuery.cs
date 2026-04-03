using InclusiON.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        SortField? SortBy,
        string SortDirection,
        List<int>? InstitutionIds = null
    );
    
}
