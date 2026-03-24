namespace InclusiON.Application.UseCases.Assignments.Commands
{
    public record AssignInstitutionCommand(
        Guid ProfessionalId,
        int InstitutionId
    );
}
