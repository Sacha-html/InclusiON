namespace InclusiON.Application.UseCases.Assignments.Commands
{
    /// <summary>
    /// Comando para dar de baja un aula: la desactiva y desvincula a sus alumnos del aula
    /// (los alumnos quedan sin aula asignada pero siguen asignados al profesional).
    /// </summary>
    public record DeactivateClassroomCommand(
        Guid ProfessionalId,
        Guid ClassroomId
    );
}
