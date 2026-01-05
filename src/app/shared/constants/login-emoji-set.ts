/**
 * Set de emojis disponibles para login visual.
 * Organizados por categorias para facilitar la seleccion.
 */
export const LoginEmojiSet = {
  /** Emojis de animales */
  animals: ['🐕', '🐈', '🐦', '🐟', '🦋', '🐢', '🐰', '🐻', '🦁', '🐘'],

  /** Emojis de objetos cotidianos */
  objects: ['🏠', '🚗', '⚽', '🎵', '📱', '🎂', '🎁', '🔑', '📚', '✏️'],

  /** Emojis de naturaleza */
  nature: ['🌈', '☀️', '🌙', '⭐', '🌸', '🌊', '🌲', '🍀', '🔥', '❄️'],

  /** Emojis de comida */
  food: ['🍎', '🍕', '🍦', '🍪', '🥤', '🍌', '🍓', '🍩', '🧁', '🍫'],

  /** Emojis de emociones y celebracion */
  emotions: ['😊', '😂', '🥰', '😎', '🤗', '👍', '💪', '🎉', '🏆', '❤️'],

  /** Todos los emojis disponibles */
  get all(): string[] {
    return [
      ...this.animals,
      ...this.objects,
      ...this.nature,
      ...this.food,
      ...this.emotions,
    ];
  },

  /** Longitud de la secuencia de login (4 emojis) */
  SEQUENCE_LENGTH: 4,

  /** Cantidad de emojis a mostrar en la grilla de seleccion */
  DISPLAY_COUNT: 9,

  /** Total de emojis disponibles */
  get totalCount(): number {
    return this.all.length;
  },
};

/** Tipo para categorias de emojis */
export type EmojiCategory = 'animals' | 'objects' | 'nature' | 'food' | 'emotions';
