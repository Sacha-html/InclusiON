namespace InclusiON.Application.UseCases.Roadmap.Commands
{
    /// <summary>
    /// Asigna directamente la actividad de un roadmap al alumno, sin pasar por encryptedId.
    /// </summary>
    public record AssignFromRoadmapCommand(
        int     PersonRoadmapActivityId,
        Guid    PersonId,
        Guid    AssignedByProfessionalId,
        DateTime? DueDate,
        bool    IsEvaluationActivity
    );
}
