using System;

namespace InclusiON.Application.UseCases.Reports.Commands
{
    public record AdminDeleteReportCommand(int ReportId, Guid AdminUserId);
}
