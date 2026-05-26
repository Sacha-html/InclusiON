namespace InclusiON.Application.Interfaces.Infrastructure
{
    public interface IPasswordResetConfig
    {
        int TokenExpiryMinutes { get; }
    }
}
