namespace InclusiON.Application.UseCases.Assignments.Commands
{
    public record TransferStudentCommand(
        Guid PersonId,
        Guid FromProfessionalId,
        Guid ToProfessionalId,
        Guid AdminUserId,
        string AdminRole
    );
}
