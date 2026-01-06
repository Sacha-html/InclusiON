namespace InclusiON.ApplicationBusiness.UseCases.Auth.Commands
{
    /// <summary>
    /// Comando para login automatico desde dispositivo confiable.
    /// </summary>
    public record TrustedDeviceLoginCommand(
        Guid UserId,
        string DeviceId,
        string? DeviceToken = null
    );
}
