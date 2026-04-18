namespace InclusiON.Application.Interfaces.Infrastructure
{
    public interface IEncryptionService
    {
        string Encrypt(string plaintext);
        string Decrypt(string ciphertext);
    }
}
