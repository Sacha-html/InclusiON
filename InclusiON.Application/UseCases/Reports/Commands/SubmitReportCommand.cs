namespace InclusiON.Application.UseCases.Reports.Commands
{
    public record SubmitReportCommand(int ReportId, Guid ProfessionalId);
}
