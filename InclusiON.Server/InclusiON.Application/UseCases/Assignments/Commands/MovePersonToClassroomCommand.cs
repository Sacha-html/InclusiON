namespace InclusiON.Application.UseCases.Assignments.Commands
{
    public record MovePersonToClassroomCommand(
        Guid ProfessionalId,
        Guid PersonId,
        Guid? ClassroomId
    );
}
