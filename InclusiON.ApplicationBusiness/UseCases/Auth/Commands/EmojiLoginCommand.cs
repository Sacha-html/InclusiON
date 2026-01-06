namespace InclusiON.ApplicationBusiness.UseCases.Auth.Commands
{
    /// <summary>
    /// Comando para login con secuencia de emojis.
    /// El usuario selecciona 3 emojis en el orden correcto.
    /// </summary>
    public record EmojiLoginCommand(
        Guid UserId,
        string[] EmojiSequence,
        string? DeviceId = null,
        bool RememberDevice = false
    );
}
