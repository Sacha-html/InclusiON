namespace InclusiON.ApplicationBusiness.UseCases.Auth.Commands
{
    /// <summary>
    /// Comando para login con secuencia de emojis.
    /// El usuario selecciona 4 emojis en el orden correcto.
    /// </summary>
    public record EmojiLoginCommand(
        int UserId,
        string[] EmojiSequence,
        string? DeviceId = null,
        bool RememberDevice = false
    );
}
