import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-actor-avatar',
  standalone: true,
  templateUrl: './actor-avatar.component.html',
  styleUrls: ['./actor-avatar.component.scss'],
})
export class ActorAvatarComponent {
  @Input({ required: true }) name!: string | null | undefined;
  /** Explicit background color from model (e.g. person.avatarColor). Auto-generated if omitted. */
  @Input() color?: string | null;
  @Input() size: 'sm' | 'md' | 'lg' = 'md';

  /**
   * Curated palette — all verified WCAG AAA (≥7:1) against white text (#ffffff).
   * Contrast ratios computed with WCAG 2.x relative luminance formula.
   * Colors with L between 0.1–0.3 excluded: neither white nor black reaches 7:1.
   */
  private static readonly AAA_PALETTE = [
    '#0D47A1', // blue-900      ~ 8.6:1
    '#7B1FA2', // purple-800    ~ 8.2:1
    '#1B5E20', // green-900     ~ 7.9:1
    '#8B0000', // dark-red      ~10.0:1
    '#4527A0', // deep-purple   ~10.2:1
    '#006064', // cyan-900      ~ 7.4:1
    '#880E4F', // pink-900      ~ 9.5:1
    '#4E342E', // brown-700     ~11.3:1
    '#004D40', // teal-900      ~ 9.8:1
    '#37474F', // blue-grey-800 ~ 9.6:1
    '#283593', // indigo-800    ~10.4:1
    '#5D1F00', // deep-orange   ~12.6:1
  ];

  get initials(): string {
    if (!this.name) return '?';
    const parts = this.name.trim().split(/\s+/).filter(Boolean);
    if (parts.length >= 2) return (parts[0][0] + parts[1][0]).toUpperCase();
    return this.name.slice(0, 2).toUpperCase();
  }

  get bgColor(): string {
    if (this.color) return this.color;
    return this.generateColor(this.name || '');
  }

  /** Picks white or black text — whichever gives higher WCAG contrast ratio. */
  get textColor(): string {
    const L = this.relativeLuminance(this.bgColor);
    if (L === null) return '#ffffff';
    const contrastWhite = 1.05 / (L + 0.05);
    const contrastBlack = (L + 0.05) / 0.05;
    return contrastWhite >= contrastBlack ? '#ffffff' : '#000000';
  }

  /** WCAG relative luminance for hex colors. Returns null for unsupported formats. */
  private relativeLuminance(color: string): number | null {
    if (!color.startsWith('#')) return null;
    const hex = color.replace('#', '');
    if (hex.length < 6) return null;
    const r = parseInt(hex.substring(0, 2), 16) / 255;
    const g = parseInt(hex.substring(2, 4), 16) / 255;
    const b = parseInt(hex.substring(4, 6), 16) / 255;
    const toLinear = (c: number) =>
      c <= 0.04045 ? c / 12.92 : Math.pow((c + 0.055) / 1.055, 2.4);
    return 0.2126 * toLinear(r) + 0.7152 * toLinear(g) + 0.0722 * toLinear(b);
  }

  private generateColor(name: string): string {
    let hash = 0;
    for (let i = 0; i < name.length; i++) {
      hash = name.charCodeAt(i) + ((hash << 5) - hash);
    }
    return ActorAvatarComponent.AAA_PALETTE[Math.abs(hash) % ActorAvatarComponent.AAA_PALETTE.length];
  }
}
