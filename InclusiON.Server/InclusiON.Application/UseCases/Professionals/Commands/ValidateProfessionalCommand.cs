namespace InclusiON.Application.UseCases.Professionals.Commands
{
    /// <summary>
    /// Comando para validar (aprobar o rechazar) un profesional registrado.
    /// </summary>
    public record ValidateProfessionalCommand(
        Guid ProfessionalId,
        bool IsApproved,
        string? Observation
    );
}