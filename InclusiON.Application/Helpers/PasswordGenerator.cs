using System.Security.Cryptography;

namespace InclusiON.Application.Helpers
{
    /// <summary>
    /// Generador de contrasenas temporales para nuevos usuarios.
    /// </summary>
    public static class PasswordGenerator
    {
        /// <summary>
        /// Genera una contrasena temporal segura que cumple con los requisitos de Identity.
        /// Formato: 12 caracteres con mayusculas, minusculas, digitos y caracteres especiales.
        /// </summary>
        public static string GenerateTemporary()
        {
            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string special = "!@#$%^&*";

            Span<char> password = stackalloc char[12];

            // Garantizar al menos uno de cada tipo requerido
            password[0] = upper[RandomNumberGenerator.GetInt32(upper.Length)];
            password[1] = lower[RandomNumberGenerator.GetInt32(lower.Length)];
            password[2] = digits[RandomNumberGenerator.GetInt32(digits.Length)];
            password[3] = special[RandomNumberGenerator.GetInt32(special.Length)];

            var all = upper + lower + digits + special;
            for (int i = 4; i < password.Length; i++)
            {
                password[i] = all[RandomNumberGenerator.GetInt32(all.Length)];
            }

            // Fisher-Yates shuffle
            for (int i = password.Length - 1; i > 0; i--)
            {
                int j = RandomNumberGenerator.GetInt32(i + 1);
                (password[i], password[j]) = (password[j], password[i]);
            }

            return new string(password);
        }
    }
}
