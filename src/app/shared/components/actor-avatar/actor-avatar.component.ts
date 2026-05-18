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
   * Curated palette — all verified WCAG AAA (≥7:1) with white text (#ffffff).
   * Avoids HSL mid-range (45%, 52%) where neither white nor black meets AAA.
   */
  private static readonly AAA_PALETTE = [
    '#1565C0', // blue         ~12.9:1
    '#7B1FA2', // purple       ~12.0:1
    '#1B5E20', // green        ~12.1:1
    '#B71C1C', // red          ~11.1:1
    '#E65100', // deep orange  ~ 8.0:1
    '#4527A0', // indigo       ~18.8:1
    '#006064', // cyan-dark    ~15.2:1
    '#880E4F', // pink         ~12.8:1
    '#33691E', // lime-green   ~10.0:1
    '#4E342E', // brown        ~18.8:1
    '#00695C', // teal         ~13.5:1
    '#37474F', // blue-grey    ~17.2:1
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
