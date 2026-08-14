using InclusiON.Application.Interfaces.Infrastructure;
using Isopoh.Cryptography.Argon2;
using Microsoft.Extensions.Logging;

namespace InclusiON.Infrastructure.Authentication
{
    /// <summary>
    /// Hasher de PINs basado en Argon2id (OWASP recomendado para credenciales de baja entropía).
    /// Soporta verificación de hashes BCrypt legacy para migración transparente.
    ///
    /// Parámetros Argon2id (perfil interactivo OWASP):
    ///   Memory = 64 MB, Iterations = 3, Parallelism = 1
    /// Tiempo esperado ≈ 80–150ms — aceptable para login pero costoso para ataques offline.
    ///
    /// Migración lazy: al verificar un hash BCrypt exitosamente, el caller debe
    /// re-hashear y persistir el nuevo hash Argon2id (indicado por needsRehash = true).
    /// </summary>
    public class Argon2idPinHasher : IPinHasher
    {
        private const int MemorySize    = 65536; // 64 MB
        private const int Iterations    = 3;
        private const int Parallelism   = 1;
        private const int HashLength    = 32;    // 256 bits
        private const int SaltLength    = 16;    // 128 bits

        private readonly ILogger<Argon2idPinHasher> _logger;

        public Argon2idPinHasher(ILogger<Argon2idPinHasher> logger)
        {
            _logger = logger;
        }

        public string Hash(string pin)
        {
            var salt = new byte[SaltLength];
            System.Security.Cryptography.RandomNumberGenerator.Fill(salt);

            var config = new Argon2Config
            {
                Type        = Argon2Type.DataIndependentAddressing, // Argon2id
                Version     = Argon2Version.Nineteen,
                MemoryCost  = MemorySize,
                TimeCost    = Iterations,
                Lanes       = Parallelism,
                Threads     = Parallelism,
                Password    = System.Text.Encoding.UTF8.GetBytes(pin),
                Salt        = salt,
                HashLength  = HashLength
            };

            using var argon2 = new Argon2(config);
            using var hash   = argon2.Hash();
            return config.EncodeString(hash.Buffer);
        }

        public bool Verify(string storedHash, string pin, out bool needsRehash)
        {
            needsRehash = false;

            if (string.IsNullOrEmpty(storedHash))
                return false;

            // Hash BCrypt legacy — formato: $2a$..., $2b$..., $2y$..., $2x$...
            if (storedHash.StartsWith("$2"))
            {
                try
                {
                    var valid = BCrypt.Net.BCrypt.Verify(pin, storedHash);
                    needsRehash = valid; // migrar a Argon2id en el próximo login exitoso
                    return valid;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error verificando hash BCrypt legacy");
                    return false;
                }
            }

            // Hash Argon2id — formato: $argon2id$...
            try
            {
                return Argon2.Verify(storedHash, pin);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error verificando hash Argon2id");
                return false;
            }
        }
    }
}
