namespace InclusiON.Application.UseCases.Diagnoses.Queries
{
    public record GetDiagnosesQuery(Guid PersonId, int Page = 1, int PageSize = 10);
}
