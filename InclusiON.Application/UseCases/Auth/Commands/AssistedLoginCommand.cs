namespace InclusiON.Application.UseCases.Auth.Commands
{
    /// <summary>
    /// Comando para login asistido.
    /// Un profesional o familiar autoriza el acceso de una persona con discapacidad
    /// usando sus credenciales de email y contrasena.
    /// </summary>
    public record AssistedLoginCommand(
        Guid UserId,
        string SupervisorEmail,
        string SupervisorPassword,
        string? DeviceId = null
    );
}
