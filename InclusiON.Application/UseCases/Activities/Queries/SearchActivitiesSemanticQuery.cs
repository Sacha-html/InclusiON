namespace InclusiON.Application.UseCases.Activities.Queries
{
    public record SearchActivitiesSemanticQuery(
        Guid ProfessionalId,
        string Text,
        int Limit
    );
}
