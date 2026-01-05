namespace InclusiON.ApplicationBusiness.UseCases.Auth.Commands
{
    /// <summary>
    /// Comando para login automatico desde dispositivo confiable.
    /// </summary>
    public record TrustedDeviceLoginCommand(
        int UserId,
        string DeviceId,
        string? DeviceToken = null
    );
}
