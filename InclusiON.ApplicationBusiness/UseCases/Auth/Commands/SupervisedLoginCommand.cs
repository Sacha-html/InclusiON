namespace InclusiON.ApplicationBusiness.UseCases.Auth.Commands
{
    /// <summary>
    /// Comando para login supervisado.
    /// Un profesional o familiar autoriza el acceso del usuario.
    /// </summary>
    public record SupervisedLoginCommand(
        Guid UserId,
        Guid SupervisorId,
        string SupervisorPin,
        string? DeviceId = null,
        string? SessionReason = null
    );
}
