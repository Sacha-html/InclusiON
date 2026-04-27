namespace InclusiON.Data.Converters
{
    // Puente estático entre InclusiON.Data (EF Core) e InclusiON.Infrastructure (AesGcmEncryptionService).
    // Se inicializa desde Infrastructure.DependencyInjection antes del primer uso del DbContext.
    public static class EncryptionAccessor
    {
        private static Func<string, string>? _encrypt;
        private static Func<string, string>? _decrypt;

        public static Func<string, string> Encrypt =>
            _encrypt ?? throw new InvalidOperationException("EncryptionAccessor not initialized.");

        public static Func<string, string> Decrypt =>
            _decrypt ?? throw new InvalidOperationException("EncryptionAccessor not initialized.");

        public static void Initialize(Func<string, string> encrypt, Func<string, string> decrypt)
        {
            _encrypt = encrypt;
            _decrypt = decrypt;
        }
    }
}
