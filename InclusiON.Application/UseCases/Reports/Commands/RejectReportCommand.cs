namespace InclusiON.Application.UseCases.Reports.Commands
{
    public record RejectReportCommand(int ReportId, Guid AdminUserId, string Comment);
}
