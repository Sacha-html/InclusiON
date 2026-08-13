namespace InclusiON.Application.UseCases.Assignments.Commands
{
    /// <summary>
    /// Comando para renombrar un aula existente.
    /// </summary>
    public record UpdateClassroomCommand(
        Guid ProfessionalId,
        Guid ClassroomId,
        string Name
    );
}
