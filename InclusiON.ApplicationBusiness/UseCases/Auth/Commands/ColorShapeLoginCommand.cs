namespace InclusiON.ApplicationBusiness.UseCases.Auth.Commands
{
    /// <summary>
    /// Comando para login con seleccion de color y forma.
    /// </summary>
    public record ColorShapeLoginCommand(
        Guid UserId,
        int ColorShapeId,
        string? DeviceId = null,
        bool RememberDevice = false
    );
}
