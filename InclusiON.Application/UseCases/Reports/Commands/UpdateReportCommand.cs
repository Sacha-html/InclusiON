namespace InclusiON.Application.UseCases.Reports.Commands
{
    public record UpdateReportCommand(
        int ReportId,
        Guid ProfessionalId,
        string Title,
        string Content,
        int ReportTypeId,
        DateTime ReportDate,
        DateTime? PeriodStartDate,
        DateTime? PeriodEndDate,
        string? AchievedGoals,
        string? AreasToReinforce,
        string? FutureRecommendations,
        string? NextObjectives
    );
}
