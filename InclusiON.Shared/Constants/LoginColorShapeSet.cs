namespace InclusiON.Shared.Constants
{
    /// <summary>
    /// Set de colores y formas disponibles para login visual.
    /// 24 combinaciones: 6 colores x 4 formas.
    /// </summary>
    public static class LoginColorShapeSet
    {
        /// <summary>
        /// Todas las combinaciones de color y forma disponibles.
        /// </summary>
        public static readonly ColorShape[] Items = new[]
        {
            // Rojos
            new ColorShape("RED_CIRCLE", "#F44336", "Circulo", "circle", "Rojo"),
            new ColorShape("RED_SQUARE", "#F44336", "Cuadrado", "square", "Rojo"),
            new ColorShape("RED_TRIANGLE", "#F44336", "Triangulo", "triangle", "Rojo"),
            new ColorShape("RED_STAR", "#F44336", "Estrella", "star", "Rojo"),

            // Azules
            new ColorShape("BLUE_CIRCLE", "#2196F3", "Circulo", "circle", "Azul"),
            new ColorShape("BLUE_SQUARE", "#2196F3", "Cuadrado", "square", "Azul"),
            new ColorShape("BLUE_TRIANGLE", "#2196F3", "Triangulo", "triangle", "Azul"),
            new ColorShape("BLUE_STAR", "#2196F3", "Estrella", "star", "Azul"),

            // Verdes
            new ColorShape("GREEN_CIRCLE", "#4CAF50", "Circulo", "circle", "Verde"),
            new ColorShape("GREEN_SQUARE", "#4CAF50", "Cuadrado", "square", "Verde"),
            new ColorShape("GREEN_TRIANGLE", "#4CAF50", "Triangulo", "triangle", "Verde"),
            new ColorShape("GREEN_STAR", "#4CAF50", "Estrella", "star", "Verde"),

            // Amarillos
            new ColorShape("YELLOW_CIRCLE", "#FFEB3B", "Circulo", "circle", "Amarillo"),
            new ColorShape("YELLOW_SQUARE", "#FFEB3B", "Cuadrado", "square", "Amarillo"),
            new ColorShape("YELLOW_TRIANGLE", "#FFEB3B", "Triangulo", "triangle", "Amarillo"),
            new ColorShape("YELLOW_STAR", "#FFEB3B", "Estrella", "star", "Amarillo"),

            // Naranjas
            new ColorShape("ORANGE_CIRCLE", "#FF9800", "Circulo", "circle", "Naranja"),
            new ColorShape("ORANGE_SQUARE", "#FF9800", "Cuadrado", "square", "Naranja"),
            new ColorShape("ORANGE_TRIANGLE", "#FF9800", "Triangulo", "triangle", "Naranja"),
            new ColorShape("ORANGE_STAR", "#FF9800", "Estrella", "star", "Naranja"),

            // Violetas
            new ColorShape("PURPLE_CIRCLE", "#9C27B0", "Circulo", "circle", "Violeta"),
            new ColorShape("PURPLE_SQUARE", "#9C27B0", "Cuadrado", "square", "Violeta"),
            new ColorShape("PURPLE_TRIANGLE", "#9C27B0", "Triangulo", "triangle", "Violeta"),
            new ColorShape("PURPLE_STAR", "#9C27B0", "Estrella", "star", "Violeta"),
        };

        /// <summary>
        /// Longitud de la secuencia de login (4 elementos).
        /// </summary>
        public const int SequenceLength = 4;

        /// <summary>
        /// Cantidad de elementos a mostrar en la grilla de seleccion.
        /// </summary>
        public const int DisplayCount = 9;

        /// <summary>
        /// Total de combinaciones disponibles.
        /// </summary>
        public static int TotalCount => Items.Length;
    }

    /// <summary>
    /// Representa una combinacion de color y forma para login visual.
    /// </summary>
    public class ColorShape
    {
        /// <summary>
        /// Codigo unico de la combinacion (ej: RED_CIRCLE).
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Color en formato hexadecimal (ej: #F44336).
        /// </summary>
        public string HexColor { get; }

        /// <summary>
        /// Nombre de la forma en espanol (ej: Circulo).
        /// </summary>
        public string ShapeName { get; }

        /// <summary>
        /// Tipo de forma para renderizado (circle, square, triangle, star).
        /// </summary>
        public string ShapeType { get; }

        /// <summary>
        /// Nombre del color en espanol (ej: Rojo).
        /// </summary>
        public string ColorName { get; }

        /// <summary>
        /// Nombre para mostrar al usuario (ej: Circulo Rojo).
        /// </summary>
        public string DisplayName => $"{ShapeName} {ColorName}";

        public ColorShape(string code, string hexColor, string shapeName, string shapeType, string colorName)
        {
            Code = code;
            HexColor = hexColor;
            ShapeName = shapeName;
            ShapeType = shapeType;
            ColorName = colorName;
        }
    }
}
