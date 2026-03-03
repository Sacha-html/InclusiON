namespace InclusiON.Shared.Constants
{
    /// <summary>
    /// Set de emojis disponibles para login visual.
    /// Organizados por categorias para facilitar la seleccion.
    /// </summary>
    public static class LoginEmojiSet
    {
        /// <summary>
        /// Emojis de animales.
        /// </summary>
        public static readonly string[] Animals = new[]
        {
            "\U0001F415", // 🐕 Perro
            "\U0001F408", // 🐈 Gato
            "\U0001F426", // 🐦 Pajaro
            "\U0001F41F", // 🐟 Pez
            "\U0001F98B", // 🦋 Mariposa
            "\U0001F422", // 🐢 Tortuga
            "\U0001F430", // 🐰 Conejo
            "\U0001F43B", // 🐻 Oso
            "\U0001F981", // 🦁 Leon
            "\U0001F418"  // 🐘 Elefante
        };

        /// <summary>
        /// Emojis de objetos cotidianos.
        /// </summary>
        public static readonly string[] Objects = new[]
        {
            "\U0001F3E0", // 🏠 Casa
            "\U0001F697", // 🚗 Auto
            "\u26BD",     // ⚽ Pelota
            "\U0001F3B5", // 🎵 Musica
            "\U0001F4F1", // 📱 Celular
            "\U0001F382", // 🎂 Torta
            "\U0001F381", // 🎁 Regalo
            "\U0001F511", // 🔑 Llave
            "\U0001F4DA", // 📚 Libros
            "\u270F\uFE0F" // ✏️ Lapiz
        };

        /// <summary>
        /// Emojis de naturaleza.
        /// </summary>
        public static readonly string[] Nature = new[]
        {
            "\U0001F308", // 🌈 Arcoiris
            "\u2600\uFE0F", // ☀️ Sol
            "\U0001F319", // 🌙 Luna
            "\u2B50",     // ⭐ Estrella
            "\U0001F338", // 🌸 Flor
            "\U0001F30A", // 🌊 Ola
            "\U0001F332", // 🌲 Arbol
            "\U0001F340", // 🍀 Trebol
            "\U0001F525", // 🔥 Fuego
            "\u2744\uFE0F" // ❄️ Nieve
        };

        /// <summary>
        /// Emojis de comida.
        /// </summary>
        public static readonly string[] Food = new[]
        {
            "\U0001F34E", // 🍎 Manzana
            "\U0001F355", // 🍕 Pizza
            "\U0001F366", // 🍦 Helado
            "\U0001F36A", // 🍪 Galleta
            "\U0001F964", // 🥤 Bebida
            "\U0001F34C", // 🍌 Banana
            "\U0001F353", // 🍓 Frutilla
            "\U0001F369", // 🍩 Dona
            "\U0001F9C1", // 🧁 Cupcake
            "\U0001F36B"  // 🍫 Chocolate
        };

        /// <summary>
        /// Emojis de emociones y celebracion.
        /// </summary>
        public static readonly string[] Emotions = new[]
        {
            "\U0001F60A", // 😊 Sonrisa
            "\U0001F602", // 😂 Risa
            "\U0001F970", // 🥰 Amor
            "\U0001F60E", // 😎 Cool
            "\U0001F917", // 🤗 Abrazo
            "\U0001F44D", // 👍 Pulgar arriba
            "\U0001F4AA", // 💪 Fuerza
            "\U0001F389", // 🎉 Fiesta
            "\U0001F3C6", // 🏆 Trofeo
            "\u2764\uFE0F" // ❤️ Corazon
        };

        /// <summary>
        /// Todos los emojis disponibles.
        /// </summary>
        public static string[] All => Animals
            .Concat(Objects)
            .Concat(Nature)
            .Concat(Food)
            .Concat(Emotions)
            .ToArray();

        /// <summary>
        /// Longitud de la secuencia de login (4 emojis).
        /// </summary>
        public const int SequenceLength = 4;

        /// <summary>
        /// Cantidad de emojis a mostrar en la grilla de seleccion.
        /// </summary>
        public const int DisplayCount = 9;

        /// <summary>
        /// Total de emojis disponibles.
        /// </summary>
        public static int TotalCount => All.Length;
    }
}
