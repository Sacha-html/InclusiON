namespace InclusiON.Application.UseCases.Auth.Queries
{
    /// <summary>
    /// Query para identificar un usuario antes del login.
    /// Devuelve el metodo de login configurado sin revelar datos sensibles.
    /// </summary>
    public record IdentifyUserQuery(
        string Identifier,
        string? DeviceId = null,
        string? UserType = null
    );
}
