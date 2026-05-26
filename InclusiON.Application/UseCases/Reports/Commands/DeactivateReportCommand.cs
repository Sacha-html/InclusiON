namespace InclusiON.Application.UseCases.Reports.Commands
{
    public record DeactivateReportCommand(int ReportId, Guid ProfessionalId);
}
