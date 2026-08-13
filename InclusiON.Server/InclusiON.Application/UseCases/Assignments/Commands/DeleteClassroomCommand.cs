namespace InclusiON.Application.UseCases.Assignments.Commands
{
    /// <summary>
    /// Comando para eliminar permanentemente un aula vacía.
    /// Solo se permite si el aula no tiene alumnos activos asignados.
    /// </summary>
    public record DeleteClassroomCommand(
        Guid ProfessionalId,
        Guid ClassroomId
    );
}
