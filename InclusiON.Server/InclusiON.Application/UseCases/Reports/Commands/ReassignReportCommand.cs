using System;

namespace InclusiON.Application.UseCases.Reports.Commands
{
    public record ReassignReportCommand(int ReportId, Guid NewProfessionalId, Guid AdminUserId);
}
