namespace InclusiON.Application.UseCases.Activities.Queries
{
    public record GetCompatiblePersonsQuery(
        int ActivityId,
        Guid ProfessionalId,
        int Limit = 10
    );
}