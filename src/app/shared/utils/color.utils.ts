/**
 * Given a hex color string (e.g. '#1B5E20'), returns '#000000' or '#ffffff'
 * whichever has better WCAG contrast against it (luminance threshold 0.179).
 *
 * Falls back to '#ffffff' for non-hex values (CSS vars, named colors, etc.).
 */
export function contrastTextColor(hex: string): '#000000' | '#ffffff' {
  const clean = hex.replace(/^#/, '');
  if (clean.length !== 6 || /[^0-9a-fA-F]/.test(clean)) return '#ffffff';

  const r = parseInt(clean.slice(0, 2), 16);
  const g = parseInt(clean.slice(2, 4), 16);
  const b = parseInt(clean.slice(4, 6), 16);

  const toLinear = (c: number): number => {
    const s = c / 255;
    return s <= 0.04045 ? s / 12.92 : Math.pow((s + 0.055) / 1.055, 2.4);
  };

  const L = 0.2126 * toLinear(r) + 0.7152 * toLinear(g) + 0.0722 * toLinear(b);
  return L > 0.179 ? '#000000' : '#ffffff';
}
