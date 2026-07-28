namespace InclusiON.Application.UseCases.Persons.Queries
{
    public record GetRecommendedActivitiesQuery(
        Guid PersonId,
        Guid ProfessionalId,
        int Limit = 10
    );
}