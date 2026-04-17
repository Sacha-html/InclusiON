import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { IconDirective } from '@coreui/icons-angular';

@Component({
  selector: 'app-big-button',
  standalone: true,
  imports: [IconDirective],
  template: `
    <button
      type="button"
      class="big-button"
      [style.--btn-color]="color"
      [style.--btn-bg]="bgColor"
      [attr.aria-label]="ariaLabel || label"
      [disabled]="disabled"
      (click)="handleClick()">
      @if (icon) {
        <div class="btn-icon">
          <svg cIcon [name]="icon" size="xxl" aria-hidden="true" focusable="false"></svg>
        </div>
      }
      @if (image) {
        <img [src]="image" alt="" class="btn-image" />
      }
      <span class="btn-label">{{ label }}</span>
    </button>
  `,
  styles: [`
    .big-button {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      gap: 12px;
      padding: 24px;
      min-width: 140px;
      min-height: 140px;
      border: 2px solid var(--a11y-border, #E0E0E0);
      border-radius: 24px;
      background: var(--btn-bg, var(--a11y-surface, #FFFFFF));
      color: var(--btn-color, var(--a11y-text, #212121));
      cursor: pointer;
      transition: transform 0.15s ease, box-shadow 0.15s ease;
      box-shadow: 0 4px 16px rgba(0, 0, 0, 0.1);
    }

    @media (prefers-reduced-motion: reduce) {
      .big-button {
        transition: none;
      }
      .big-button:hover:not(:disabled),
      .big-button:active:not(:disabled) {
        transform: none;
      }
    }

    .big-button:hover:not(:disabled) {
      transform: scale(1.03);
      box-shadow: 0 6px 24px rgba(0, 0, 0, 0.15);
    }

    .big-button:active:not(:disabled) {
      transform: scale(0.98);
    }

    .big-button:focus-visible {
      outline: 4px solid var(--a11y-focus-accent, #0D47A1);
      outline-offset: 4px;
    }

    .big-button:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

    .btn-icon {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 72px;
      height: 72px;
      border-radius: 20px;
      background: var(--btn-color, #2196F3);
      color: var(--a11y-primary-text, white);

      svg {
        width: 40px;
        height: 40px;
      }
    }

    .btn-image {
      width: 72px;
      height: 72px;
      object-fit: contain;
    }

    .btn-label {
      font-size: 20px;
      font-weight: 600;
      text-align: center;
      line-height: 1.2;
    }

    :host-context([data-profile="high-contrast"]) .big-button {
      border: 3px solid var(--btn-color);
      background: var(--a11y-bg, #000);
      color: var(--a11y-text, #fff);

      .btn-label {
        color: var(--a11y-text, #fff);
      }
    }

    :host-context([data-color-mode="dark"]) .big-button {
      background: var(--a11y-surface, #2a2a3e);
      box-shadow: 0 4px 16px rgba(0, 0, 0, 0.3);

      .btn-label {
        color: var(--a11y-text, #f5f5f5);
      }
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BigButtonComponent {
  @Input() label = '';
  @Input() icon?: string;
  @Input() image?: string;
  @Input() color = '#2196F3';
  @Input() bgColor = '#FFFFFF';
  @Input() disabled = false;
  @Input() ariaLabel?: string;

  @Output() buttonClick = new EventEmitter<void>();

  handleClick(): void {
    if (!this.disabled) {
      this.buttonClick.emit();
    }
  }
}
