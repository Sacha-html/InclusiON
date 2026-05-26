namespace InclusiON.Application.UseCases.Activities.Commands
{
    public record CompleteActivityResponseCommand(
        int AssignmentId,
        int ResponseId,
        Guid PersonId,
        decimal SuccessPercentage,
        int TimeSpentSeconds,
        bool RequiredSupport,
        int? FrustrationLevel,
        string? ResponsePattern,
        string? Observations
    );
}
