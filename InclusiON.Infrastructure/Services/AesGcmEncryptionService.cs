using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using InclusiON.Application.Interfaces.Infrastructure;

namespace InclusiON.Infrastructure.Services
{
    // AES-256-GCM: 12-byte nonce, 16-byte tag, base64-encoded, prefixed with "ENC:"
    // Encrypted format: ENC:<base64(nonce[12] + ciphertext[N] + tag[16])>
    // Plaintext values without the prefix pass through Decrypt unchanged (lazy migration fallback).
    public class AesGcmEncryptionService : IEncryptionService
    {
        private const string Prefix = "ENC:";
        private const int NonceSize = 12;
        private const int TagSize   = 16;

        private readonly byte[] _key;

        public AesGcmEncryptionService(IConfiguration configuration)
        {
            var raw = configuration["EncryptionSettings:Key"]
                ?? throw new InvalidOperationException("EncryptionSettings:Key is missing.");

            _key = Convert.FromBase64String(raw);

            if (_key.Length != 32)
                throw new InvalidOperationException("EncryptionSettings:Key must be a base64-encoded 32-byte (256-bit) value.");
        }

        public string Encrypt(string plaintext)
        {
            var nonce      = new byte[NonceSize];
            RandomNumberGenerator.Fill(nonce);

            var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            var ciphertext     = new byte[plaintextBytes.Length];
            var tag            = new byte[TagSize];

            using var aes = new AesGcm(_key, TagSize);
            aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);

            var combined = new byte[NonceSize + ciphertext.Length + TagSize];
            nonce.CopyTo(combined, 0);
            ciphertext.CopyTo(combined, NonceSize);
            tag.CopyTo(combined, NonceSize + ciphertext.Length);

            return Prefix + Convert.ToBase64String(combined);
        }

        public string Decrypt(string ciphertext)
        {
            if (!ciphertext.StartsWith(Prefix, StringComparison.Ordinal))
                return ciphertext; // plaintext fallback — valor pre-migración

            var combined = Convert.FromBase64String(ciphertext[Prefix.Length..]);

            var nonce  = combined[..NonceSize];
            var tag    = combined[^TagSize..];
            var ct     = combined[NonceSize..^TagSize];
            var result = new byte[ct.Length];

            using var aes = new AesGcm(_key, TagSize);
            aes.Decrypt(nonce, ct, tag, result);

            return Encoding.UTF8.GetString(result);
        }
    }
}
