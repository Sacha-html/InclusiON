namespace InclusiON.Application.UseCases.Diagnoses.Commands
{
    public record PatchDiagnosisStatusCommand(
        int DiagnosisId,
        bool IsActive,
        Guid? RequestedByProfessionalId);
}
