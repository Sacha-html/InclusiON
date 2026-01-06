using InclusiON.ApplicationBusiness.Interfaces.Infrastructure;
using BCrypt.Net;

namespace InclusiON.Infrastructure.Authentication
{
    public class PasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Password cannot be null", nameof(password));
            }

            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
        }

        public bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            if (string.IsNullOrEmpty(hashedPassword) || string.IsNullOrEmpty(providedPassword))
            {
                return false;
            }

            try
            {
                return BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword);
            }
            catch
            {
                return false;
            }
        }
    }
}
