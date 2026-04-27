using System;

namespace InclusiON.Application.UseCases.Reports.Commands
{
    public record CreateReportCommand(
        Guid PersonId,
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