namespace InclusiON.ApplicationBusiness.UseCases.Auth.Commands
{
    /// <summary>
    /// Comando para refrescar el token de acceso usando un refresh token valido.
    /// </summary>
    public record RefreshTokenCommand(
        string RefreshToken
    );
}
