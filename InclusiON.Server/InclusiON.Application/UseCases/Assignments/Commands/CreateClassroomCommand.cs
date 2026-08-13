namespace InclusiON.Application.UseCases.Assignments.Commands
{
    /// <summary>
    /// Comando para crear un aula y asociar alumnos a ella.
    /// </summary>
    public record CreateClassroomCommand(
        Guid ProfessionalId,
        string Name,
        List<Guid>? PersonIds,
        bool IsPrimaryProfessional,
        bool CanSuperviseLogin
    );
}
