namespace InclusiON.Shared.Constants
{
    /// <summary>
    /// Colores disponibles para avatares de usuarios.
    /// Usados en el metodo PROFILE_SELECT para identificar visualmente a cada persona.
    /// </summary>
    public static class AvatarColors
    {
        /// <summary>
        /// Lista de colores disponibles para avatares.
        /// </summary>
        public static readonly AvatarColor[] Items = new[]
        {
            new AvatarColor("#F44336", "Rojo"),
            new AvatarColor("#E91E63", "Rosa"),
            new AvatarColor("#9C27B0", "Violeta"),
            new AvatarColor("#673AB7", "Purpura"),
            new AvatarColor("#3F51B5", "Indigo"),
            new AvatarColor("#2196F3", "Azul"),
            new AvatarColor("#03A9F4", "Celeste"),
            new AvatarColor("#00BCD4", "Cian"),
            new AvatarColor("#009688", "Verde Azulado"),
            new AvatarColor("#4CAF50", "Verde"),
            new AvatarColor("#8BC34A", "Verde Claro"),
            new AvatarColor("#CDDC39", "Lima"),
            new AvatarColor("#FFEB3B", "Amarillo"),
            new AvatarColor("#FFC107", "Ambar"),
            new AvatarColor("#FF9800", "Naranja"),
            new AvatarColor("#FF5722", "Naranja Oscuro"),
        };

        /// <summary>
        /// Total de colores disponibles.
        /// </summary>
        public static int TotalCount => Items.Length;
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
