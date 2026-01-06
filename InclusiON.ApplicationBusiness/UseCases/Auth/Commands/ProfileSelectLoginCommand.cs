namespace InclusiON.ApplicationBusiness.UseCases.Auth.Commands
{
    /// <summary>
    /// Comando para login por seleccion de perfil visual.
    /// El usuario selecciona su avatar de una lista de perfiles.
    /// </summary>
    public record ProfileSelectLoginCommand(
        Guid UserId,
        string DeviceId,
        bool RequiresConfirmation = false,
        string? ConfirmationPin = null
    );
}
