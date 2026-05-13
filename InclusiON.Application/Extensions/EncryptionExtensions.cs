using InclusiON.Application.Interfaces.Infrastructure;

namespace InclusiON.Application.Extensions
{
    public static class EncryptionExtensions
    {
        public static string EncryptId(this IEncryptionService encryption, int id)
        {
            var encrypted = encryption.Encrypt(id.ToString());
            return encrypted.Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }

        public static bool TryDecryptId(this IEncryptionService encryption, string encryptedId, out int id)
        {
            try
            {
                var padded = encryptedId.Replace('-', '+').Replace('_', '/');
                var padding = padded.Length % 4 == 0 ? 0 : 4 - padded.Length % 4;
                padded += new string('=', padding);
                return int.TryParse(encryption.Decrypt(padded), out id);
            }
            catch
            {
                id = 0;
                return false;
            }
        }
    }
}
