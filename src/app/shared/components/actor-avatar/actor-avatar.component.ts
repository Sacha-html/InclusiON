import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-actor-avatar',
  standalone: true,
  template: `
    <div
      class="actor-avatar"
      [class.actor-avatar--sm]="size === 'sm'"
      [class.actor-avatar--lg]="size === 'lg'"
      [style.backgroundColor]="bgColor"
      [style.color]="textColor"
      [attr.aria-label]="name || undefined"
      [attr.aria-hidden]="!name ? 'true' : undefined"
    >{{ initials }}</div>
  `,
  styles: [`
    .actor-avatar {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      border-radius: 50%;
      font-weight: 600;
      flex-shrink: 0;
      width: 36px;
      height: 36px;
      font-size: 0.875rem;
      user-select: none;
    }
    .actor-avatar--sm {
      width: 28px;
      height: 28px;
      font-size: 0.75rem;
    }
    .actor-avatar--lg {
      width: 48px;
      height: 48px;
      font-size: 1.125rem;
    }
  `],
})
export class ActorAvatarComponent {
  @Input({ required: true }) name!: string | null | undefined;
  /** Explicit background color from model (e.g. person.avatarColor). Auto-generated if omitted. */
  @Input() color?: string | null;
  @Input() size: 'sm' | 'md' | 'lg' = 'md';

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

  get textColor(): string {
    const hex = this.bgColor.startsWith('#') ? this.bgColor.replace('#', '') : null;
    if (!hex || hex.length < 6) return '#ffffff';
    const r = parseInt(hex.substring(0, 2), 16);
    const g = parseInt(hex.substring(2, 4), 16);
    const b = parseInt(hex.substring(4, 6), 16);
    const luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255;
    return luminance > 0.55 ? '#000000' : '#ffffff';
  }

  private generateColor(name: string): string {
    let hash = 0;
    for (let i = 0; i < name.length; i++) {
      hash = name.charCodeAt(i) + ((hash << 5) - hash);
    }
    return `hsl(${Math.abs(hash % 360)}, 45%, 52%)`;
  }
}
