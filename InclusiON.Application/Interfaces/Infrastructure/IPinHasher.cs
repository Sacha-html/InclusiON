namespace InclusiON.Application.Interfaces.Infrastructure
{
    public interface IPinHasher
    {
        /// <summary>Hashea un PIN con Argon2id.</summary>
        string Hash(string pin);

        /// <summary>
        /// Verifica un PIN contra el hash almacenado (soporta BCrypt y Argon2id).
        /// Devuelve true si el PIN es válido; <paramref name="needsRehash"/> indica
        /// que el hash usa el algoritmo legacy (BCrypt) y debe actualizarse con Argon2id.
        /// </summary>
        bool Verify(string storedHash, string pin, out bool needsRehash);
    }
}
