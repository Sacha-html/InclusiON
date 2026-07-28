namespace InclusiON.Application.UseCases.Auth.Commands
{
    /// <summary>
    /// Comando para login de familiar/tutor.
    /// El familiar se identifica por nombre y luego ingresa su contraseña.
    /// </summary>
    public record FamilyLoginCommand(
        Guid UserId,
        string Password,
        string? DeviceId = null,
        bool RememberDevice = false
    );
}
