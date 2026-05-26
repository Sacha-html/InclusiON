namespace InclusiON.Application.UseCases.Persons.Queries
{
    public record GetSupervisorCandidatesQuery(Guid PersonId, int Page = 1, int PageSize = 50);
}
