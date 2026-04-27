namespace InclusiON.Data.Seeders
{
    // Puente estático para que DatabaseSeeder use IPinHasher sin referenciar Application.
    // Se inicializa desde Infrastructure.DependencyInjection al arrancar.
    public static class PinHashAccessor
    {
        private static Func<string, string>? _hash;

        public static Func<string, string> Hash =>
            _hash ?? throw new InvalidOperationException("PinHashAccessor not initialized.");

        public static void Initialize(Func<string, string> hash) => _hash = hash;
    }
}
