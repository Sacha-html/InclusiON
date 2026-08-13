namespace InclusiON.Application.UseCases.Assignments.Queries
{
    /// <summary>
    /// Consulta para obtener las aulas de un profesional.
    /// </summary>
    public record GetClassroomsByProfessionalQuery(Guid ProfessionalId);
}
