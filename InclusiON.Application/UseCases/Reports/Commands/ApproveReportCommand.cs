namespace InclusiON.Application.UseCases.Reports.Commands
{
    public record ApproveReportCommand(int ReportId, Guid AdminUserId);
}
