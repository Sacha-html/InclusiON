namespace InclusiON.Application.UseCases.Activities.Queries
{
    public record GetSimilarActivitiesQuery(
        Guid ProfessionalId,
        int ActivityId,
        int Limit = 5
    );
}