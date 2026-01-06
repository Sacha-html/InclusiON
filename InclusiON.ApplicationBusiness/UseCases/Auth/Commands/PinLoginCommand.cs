namespace InclusiON.ApplicationBusiness.UseCases.Auth.Commands
{
    /// <summary>
    /// Comando para login con PIN numerico.
    /// </summary>
    public record PinLoginCommand(
        Guid UserId,
        string Pin,
        string? DeviceId = null,
        bool RememberDevice = false
    );
}
