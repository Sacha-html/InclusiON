namespace InclusiON.Application.UseCases.Assignments.Commands
{
    public record RemoveInstitutionAssignmentCommand(
        Guid ProfessionalId,
        int InstitutionId
    );
}
