/**
 * Representa un color de avatar.
 */
export interface AvatarColor {
  /** Color en formato hexadecimal (ej: #F44336) */
  hex: string;
  /** Nombre del color en espanol (ej: Rojo) */
  name: string;
}

/**
 * Colores disponibles para avatares de usuarios.
 * Usados en el metodo PROFILE_SELECT para identificar visualmente a cada persona.
 */
export const AvatarColors: AvatarColor[] = [
  { hex: '#F44336', name: 'Rojo' },
  { hex: '#E91E63', name: 'Rosa' },
  { hex: '#9C27B0', name: 'Violeta' },
  { hex: '#673AB7', name: 'Purpura' },
  { hex: '#3F51B5', name: 'Indigo' },
  { hex: '#2196F3', name: 'Azul' },
  { hex: '#03A9F4', name: 'Celeste' },
  { hex: '#00BCD4', name: 'Cian' },
  { hex: '#009688', name: 'Verde Azulado' },
  { hex: '#4CAF50', name: 'Verde' },
  { hex: '#8BC34A', name: 'Verde Claro' },
  { hex: '#CDDC39', name: 'Lima' },
  { hex: '#FFEB3B', name: 'Amarillo' },
  { hex: '#FFC107', name: 'Ambar' },
  { hex: '#FF9800', name: 'Naranja' },
  { hex: '#FF5722', name: 'Naranja Oscuro' },
];

/**
 * Obtiene un color aleatorio de la lista de colores disponibles.
 */
export function getRandomAvatarColor(): AvatarColor {
  return AvatarColors[Math.floor(Math.random() * AvatarColors.length)];
}

/**
 * Obtiene un color por indice (con wrap-around).
 */
export function getAvatarColorByIndex(index: number): AvatarColor {
  return AvatarColors[index % AvatarColors.length];
}
