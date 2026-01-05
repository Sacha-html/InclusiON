/**
 * Representa una combinacion de color y forma para login visual.
 */
export interface ColorShape {
  /** Codigo unico de la combinacion (ej: RED_CIRCLE) */
  code: string;
  /** Color en formato hexadecimal (ej: #F44336) */
  hexColor: string;
  /** Nombre de la forma en espanol (ej: Circulo) */
  shapeName: string;
  /** Tipo de forma para renderizado */
  shapeType: ShapeType;
  /** Nombre del color en espanol (ej: Rojo) */
  colorName: string;
  /** Nombre para mostrar al usuario (ej: Circulo Rojo) */
  displayName: string;
}

/** Tipos de formas disponibles */
export type ShapeType = 'circle' | 'square' | 'triangle' | 'star';

/** Colores disponibles */
export type ShapeColor = 'RED' | 'BLUE' | 'GREEN' | 'YELLOW' | 'ORANGE' | 'PURPLE';

/**
 * Set de colores y formas disponibles para login visual.
 * 24 combinaciones: 6 colores x 4 formas.
 */
export const LoginColorShapeSet = {
  items: [
    // Rojos
    { code: 'RED_CIRCLE', hexColor: '#F44336', shapeName: 'Circulo', shapeType: 'circle' as ShapeType, colorName: 'Rojo', displayName: 'Circulo Rojo' },
    { code: 'RED_SQUARE', hexColor: '#F44336', shapeName: 'Cuadrado', shapeType: 'square' as ShapeType, colorName: 'Rojo', displayName: 'Cuadrado Rojo' },
    { code: 'RED_TRIANGLE', hexColor: '#F44336', shapeName: 'Triangulo', shapeType: 'triangle' as ShapeType, colorName: 'Rojo', displayName: 'Triangulo Rojo' },
    { code: 'RED_STAR', hexColor: '#F44336', shapeName: 'Estrella', shapeType: 'star' as ShapeType, colorName: 'Rojo', displayName: 'Estrella Roja' },

    // Azules
    { code: 'BLUE_CIRCLE', hexColor: '#2196F3', shapeName: 'Circulo', shapeType: 'circle' as ShapeType, colorName: 'Azul', displayName: 'Circulo Azul' },
    { code: 'BLUE_SQUARE', hexColor: '#2196F3', shapeName: 'Cuadrado', shapeType: 'square' as ShapeType, colorName: 'Azul', displayName: 'Cuadrado Azul' },
    { code: 'BLUE_TRIANGLE', hexColor: '#2196F3', shapeName: 'Triangulo', shapeType: 'triangle' as ShapeType, colorName: 'Azul', displayName: 'Triangulo Azul' },
    { code: 'BLUE_STAR', hexColor: '#2196F3', shapeName: 'Estrella', shapeType: 'star' as ShapeType, colorName: 'Azul', displayName: 'Estrella Azul' },

    // Verdes
    { code: 'GREEN_CIRCLE', hexColor: '#4CAF50', shapeName: 'Circulo', shapeType: 'circle' as ShapeType, colorName: 'Verde', displayName: 'Circulo Verde' },
    { code: 'GREEN_SQUARE', hexColor: '#4CAF50', shapeName: 'Cuadrado', shapeType: 'square' as ShapeType, colorName: 'Verde', displayName: 'Cuadrado Verde' },
    { code: 'GREEN_TRIANGLE', hexColor: '#4CAF50', shapeName: 'Triangulo', shapeType: 'triangle' as ShapeType, colorName: 'Verde', displayName: 'Triangulo Verde' },
    { code: 'GREEN_STAR', hexColor: '#4CAF50', shapeName: 'Estrella', shapeType: 'star' as ShapeType, colorName: 'Verde', displayName: 'Estrella Verde' },

    // Amarillos
    { code: 'YELLOW_CIRCLE', hexColor: '#FFEB3B', shapeName: 'Circulo', shapeType: 'circle' as ShapeType, colorName: 'Amarillo', displayName: 'Circulo Amarillo' },
    { code: 'YELLOW_SQUARE', hexColor: '#FFEB3B', shapeName: 'Cuadrado', shapeType: 'square' as ShapeType, colorName: 'Amarillo', displayName: 'Cuadrado Amarillo' },
    { code: 'YELLOW_TRIANGLE', hexColor: '#FFEB3B', shapeName: 'Triangulo', shapeType: 'triangle' as ShapeType, colorName: 'Amarillo', displayName: 'Triangulo Amarillo' },
    { code: 'YELLOW_STAR', hexColor: '#FFEB3B', shapeName: 'Estrella', shapeType: 'star' as ShapeType, colorName: 'Amarillo', displayName: 'Estrella Amarilla' },

    // Naranjas
    { code: 'ORANGE_CIRCLE', hexColor: '#FF9800', shapeName: 'Circulo', shapeType: 'circle' as ShapeType, colorName: 'Naranja', displayName: 'Circulo Naranja' },
    { code: 'ORANGE_SQUARE', hexColor: '#FF9800', shapeName: 'Cuadrado', shapeType: 'square' as ShapeType, colorName: 'Naranja', displayName: 'Cuadrado Naranja' },
    { code: 'ORANGE_TRIANGLE', hexColor: '#FF9800', shapeName: 'Triangulo', shapeType: 'triangle' as ShapeType, colorName: 'Naranja', displayName: 'Triangulo Naranja' },
    { code: 'ORANGE_STAR', hexColor: '#FF9800', shapeName: 'Estrella', shapeType: 'star' as ShapeType, colorName: 'Naranja', displayName: 'Estrella Naranja' },

    // Violetas
    { code: 'PURPLE_CIRCLE', hexColor: '#9C27B0', shapeName: 'Circulo', shapeType: 'circle' as ShapeType, colorName: 'Violeta', displayName: 'Circulo Violeta' },
    { code: 'PURPLE_SQUARE', hexColor: '#9C27B0', shapeName: 'Cuadrado', shapeType: 'square' as ShapeType, colorName: 'Violeta', displayName: 'Cuadrado Violeta' },
    { code: 'PURPLE_TRIANGLE', hexColor: '#9C27B0', shapeName: 'Triangulo', shapeType: 'triangle' as ShapeType, colorName: 'Violeta', displayName: 'Triangulo Violeta' },
    { code: 'PURPLE_STAR', hexColor: '#9C27B0', shapeName: 'Estrella', shapeType: 'star' as ShapeType, colorName: 'Violeta', displayName: 'Estrella Violeta' },
  ] as ColorShape[],

  /** Longitud de la secuencia de login (4 elementos) */
  SEQUENCE_LENGTH: 4,

  /** Cantidad de elementos a mostrar en la grilla de seleccion */
  DISPLAY_COUNT: 9,

  /** Total de combinaciones disponibles */
  get totalCount(): number {
    return this.items.length;
  },
};
