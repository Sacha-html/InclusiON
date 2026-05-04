namespace InclusiON.Application.UseCases.Activities.Queries
{
    /// <summary>
    /// Obtiene una asignación de actividad por su ID, verificando que el solicitante
    /// sea la persona asignada o el profesional que la creó.
    /// </summary>
    public record GetAssignmentByIdQuery(int AssignmentId, Guid RequesterId);
}
