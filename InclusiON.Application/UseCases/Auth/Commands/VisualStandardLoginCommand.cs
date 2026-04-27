namespace InclusiON.Application.UseCases.Auth.Commands
{
    /// <summary>
    /// Comando para login visual estandar.
    /// La persona con discapacidad se identifica por nombre y luego ingresa su contraseña.
    /// </summary>
    public record VisualStandardLoginCommand(
        Guid UserId,
        string Password,
        string? DeviceId = null,
        bool RememberDevice = false
    );
}
