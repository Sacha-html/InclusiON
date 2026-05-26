namespace InclusiON.Application.UseCases.Professionals.Queries
{
    /// <summary>
    /// Devuelve el resumen de progreso semanal del profesional autenticado.
    /// </summary>
    public record GetWeeklyProgressQuery(Guid ProfessionalId);
}
