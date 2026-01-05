namespace InclusiON.ApplicationBusiness.UseCases.Auth.Commands
{
    /// <summary>
    /// Comando para login con PIN numerico.
    /// </summary>
    public record PinLoginCommand(
        int UserId,
        string Pin,
        string? DeviceId = null,
        bool RememberDevice = false
    );
}
