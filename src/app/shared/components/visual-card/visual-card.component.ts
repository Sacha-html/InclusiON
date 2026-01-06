import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { IconDirective } from '@coreui/icons-angular';

@Component({
  selector: 'app-visual-card',
  standalone: true,
  imports: [IconDirective],
  template: `
    <article
      class="visual-card"
      [class.interactive]="interactive"
      [style.--card-accent]="accentColor"
      [attr.role]="interactive ? 'button' : 'article'"
      [attr.tabindex]="interactive ? 0 : null"
      [attr.aria-label]="ariaLabel || title"
      (click)="handleClick()"
      (keydown.enter)="handleClick()"
      (keydown.space)="handleClick()">

      <div class="card-visual">
        @if (image) {
          <img [src]="image" [alt]="title" class="card-image" />
        } @else if (icon) {
          <div class="card-icon" [style.background-color]="accentColor">
            <svg cIcon [name]="icon" size="xxl"></svg>
          </div>
        }
      </div>

      <div class="card-content">
        <h3 class="card-title">{{ title }}</h3>
        @if (subtitle) {
          <p class="card-subtitle">{{ subtitle }}</p>
        }
      </div>

      @if (badge) {
        <span class="card-badge" [style.background-color]="badgeColor">
          {{ badge }}
        </span>
      }
    </article>
  `,
  styles: [`
    .visual-card {
      position: relative;
      display: flex;
      flex-direction: column;
      padding: 24px;
      background: white;
      border-radius: 24px;
      box-shadow: 0 4px 16px rgba(0, 0, 0, 0.08);
      transition: transform 0.15s ease, box-shadow 0.15s ease;
    }

    .visual-card.interactive {
      cursor: pointer;
    }

    .visual-card.interactive:hover {
      transform: translateY(-4px);
      box-shadow: 0 8px 28px rgba(0, 0, 0, 0.12);
    }

    .visual-card:focus {
      outline: 4px solid #FFD700;
      outline-offset: 4px;
    }

    .card-visual {
      display: flex;
      justify-content: center;
      margin-bottom: 16px;
    }

    .card-image {
      width: 88px;
      height: 88px;
      object-fit: contain;
      border-radius: 20px;
    }

    .card-icon {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 80px;
      height: 80px;
      border-radius: 20px;
      color: white;

      svg {
        width: 44px;
        height: 44px;
      }
    }

    .card-content {
      text-align: center;
    }

    .card-title {
      margin: 0 0 8px;
      font-size: 22px;
      font-weight: 600;
      color: #1a1a1a;
    }

    .card-subtitle {
      margin: 0;
      font-size: 16px;
      color: #666;
    }

    .card-badge {
      position: absolute;
      top: 16px;
      right: 16px;
      padding: 6px 14px;
      border-radius: 14px;
      font-size: 14px;
      font-weight: 700;
      color: white;
    }

    :host-context([data-profile="high-contrast"]) .visual-card {
      border: 3px solid var(--card-accent);
      background: #000;
    }

    :host-context([data-profile="high-contrast"]) .card-title,
    :host-context([data-profile="high-contrast"]) .card-subtitle {
      color: #FFF;
    }

    :host-context([data-color-mode="dark"]) .visual-card {
      background: #2a2a3e;
    }

    :host-context([data-color-mode="dark"]) .card-title {
      color: #f5f5f5;
    }

    :host-context([data-color-mode="dark"]) .card-subtitle {
      color: #aaa;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class VisualCardComponent {
  @Input() title = '';
  @Input() subtitle?: string;
  @Input() icon?: string;
  @Input() image?: string;
  @Input() accentColor = '#2196F3';
  @Input() badge?: string;
  @Input() badgeColor = '#F44336';
  @Input() interactive = true;
  @Input() ariaLabel?: string;

  @Output() cardClick = new EventEmitter<void>();

  handleClick(): void {
    if (this.interactive) {
      this.cardClick.emit();
    }
  }
}
