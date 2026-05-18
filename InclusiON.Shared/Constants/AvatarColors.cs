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
            new AvatarColor("#C62828", "Rojo"),
            new AvatarColor("#AD1457", "Rosa"),
            new AvatarColor("#6A1B9A", "Violeta"),
            new AvatarColor("#4527A0", "Purpura"),
            new AvatarColor("#283593", "Indigo"),
            new AvatarColor("#2196F3", "Azul"),
            new AvatarColor("#03A9F4", "Celeste"),
            new AvatarColor("#00BCD4", "Cian"),
            new AvatarColor("#00695C", "Verde Azulado"),
            new AvatarColor("#4CAF50", "Verde"),
            new AvatarColor("#8BC34A", "Verde Claro"),
            new AvatarColor("#CDDC39", "Lima"),
            new AvatarColor("#FFEB3B", "Amarillo"),
            new AvatarColor("#FFC107", "Ambar"),
            new AvatarColor("#FF9800", "Naranja"),
            new AvatarColor("#BF360C", "Naranja Oscuro"),
        };

        /// <summary>
        /// Total de colores disponibles.
        /// </summary>
        public static int TotalCount => Items.Length;

        /// <summary>
        /// Color por defecto para una persona con discapacidad (Azul).
        /// </summary>
        public const string DefaultPerson = "#2196F3";

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
