using Microsoft.Extensions.Options;
using InclusiON.Application.Interfaces.Infrastructure;

namespace InclusiON.Infrastructure.Configuration
{
    public class PasswordResetConfig : IPasswordResetConfig
    {
        private readonly PasswordResetSettings _settings;

        public PasswordResetConfig(IOptions<PasswordResetSettings> settings)
        {
            _settings = settings.Value;
        }

        public int TokenExpiryMinutes => _settings.TokenExpiryMinutes;
    }
}
