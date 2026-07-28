namespace InclusiON.Shared.Constants
{
    /// <summary>
    /// Colores disponibles para avatares de usuarios.
    /// Usados en el metodo PROFILE_SELECT para identificar visualmente a cada persona.
    /// </summary>
    public static class AvatarColors
    {
        /// <summary>
        /// Lista de colores disponibles para avatares. Todos cumplen WCAG AAA (7:1).
        /// </summary>
        public static readonly AvatarColor[] Items = new[]
        {
            // Colores oscuros — texto blanco, verificados WCAG AAA con formula WCAG 2.x
            new AvatarColor("#8B0000", "Rojo"),          // L≈0.055 → 10.0:1 blanco
            new AvatarColor("#880E4F", "Rosa"),          // L≈0.061 → 9.5:1  blanco
            new AvatarColor("#6A1B9A", "Violeta"),       // L≈0.062 → 9.4:1  blanco
            new AvatarColor("#4527A0", "Purpura"),       // L≈0.053 → 10.2:1 blanco
            new AvatarColor("#283593", "Indigo"),        // L≈0.051 → 10.4:1 blanco
            new AvatarColor("#0D47A1", "Azul"),          // L≈0.072 → 8.6:1  blanco
            new AvatarColor("#004D40", "Verde Azulado"), // L≈0.057 → 9.8:1  blanco
            // Colores claros — texto negro, verificados WCAG AAA
            new AvatarColor("#03A9F4", "Celeste"),       // L≈0.350 → 8.0:1  negro
            new AvatarColor("#00BCD4", "Cian"),          // L≈0.407 → 9.1:1  negro
            new AvatarColor("#4CAF50", "Verde"),         // L≈0.328 → 7.6:1  negro
            new AvatarColor("#8BC34A", "Verde Claro"),   // L≈0.451 → 10.0:1 negro
            new AvatarColor("#CDDC39", "Lima"),          // L≈0.645 → 13.9:1 negro
            new AvatarColor("#FFEB3B", "Amarillo"),      // L≈0.822 → 17.4:1 negro
            new AvatarColor("#FFC107", "Ambar"),         // L≈0.594 → 12.9:1 negro
            new AvatarColor("#FF9800", "Naranja"),       // L≈0.437 → 9.7:1  negro
            new AvatarColor("#5D1F00", "Naranja Oscuro"),// L≈0.033 → 12.6:1 blanco
        };

        /// <summary>
        /// Total de colores disponibles.
        /// </summary>
        public static int TotalCount => Items.Length;

        /// <summary>
        /// Color por defecto para una persona con discapacidad (Azul oscuro WCAG AAA).
        /// </summary>
        public const string DefaultPerson = "#0D47A1";

        /// <summary>
        /// Color por defecto para profesionales (Verde).
        /// </summary>
        public const string DefaultProfessional = "#4CAF50";

        /// <summary>
        /// Color por defecto para familiares (Violeta).
        /// </summary>
        public const string DefaultFamily = "#6A1B9A";

        /// <summary>
        /// Devuelve un color aleatorio del catalogo.
        /// </summary>
        public static string Random()
        {
            return Items[System.Random.Shared.Next(Items.Length)].Hex;
        }
    }

    /// <summary>
    /// Representa un color de avatar.
    /// </summary>
    public class AvatarColor
    {
        /// <summary>
        /// Color en formato hexadecimal (ej: #F44336).
        /// </summary>
        public string Hex { get; }

        /// <summary>
        /// Nombre del color en espanol (ej: Rojo).
        /// </summary>
        public string Name { get; }

        public AvatarColor(string hex, string name)
        {
            Hex = hex;
            Name = name;
        }
    }
}
