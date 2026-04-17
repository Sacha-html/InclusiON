import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
/**
 * Componente de avatar para login visual con seleccion de perfil.
 * Muestra inicial del nombre con color de fondo configurable.
 * Accesible: incluye aria-label y soporte para teclado.
 */
@Component({
  selector: 'app-avatar',
  standalone: true,
  imports: [],
  template: `
    <button
      type="button"
      class="avatar-button"
      [class.selected]="selected"
      [attr.aria-label]="ariaLabel || name || 'Avatar'"
      [attr.aria-pressed]="selected"
      (click)="handleClick()"
      (keydown.enter)="handleClick()"
      (keydown.space)="handleClick()">
      <div
        class="avatar-circle"
        [style.width.px]="size"
        [style.height.px]="size"
        [style.background-color]="color"
        [style.font-size.px]="fontSize">
        {{ displayInitial }}
      </div>
      @if (showName && name) {
        <span class="avatar-name">{{ name }}</span>
      }
    </button>
  `,
  styles: [`
    .avatar-button {
      display: inline-flex;
      flex-direction: column;
      align-items: center;
      gap: 8px;
      padding: 8px;
      border: 3px solid transparent;
      border-radius: 12px;
      background: transparent;
      cursor: pointer;
      transition: all 0.2s ease;
      min-width: 44px;
      min-height: 44px;
    }

    @media (prefers-reduced-motion: reduce) {
      .avatar-button {
        transition: none;
      }
      .avatar-button:hover,
      .avatar-button:active {
        transform: none;
      }
    }

    .avatar-button:hover {
      background-color: rgba(0, 0, 0, 0.05);
      transform: scale(1.05);
    }

    .avatar-button:focus-visible {
      outline: 3px solid var(--a11y-focus-accent, #0D47A1);
      outline-offset: 2px;
    }

    .avatar-button.selected {
      border-color: var(--a11y-primary, #0066CC);
      background-color: color-mix(in srgb, var(--a11y-primary, #0066CC) 10%, transparent);
    }

    .avatar-button:active {
      transform: scale(0.95);
    }

    .avatar-circle {
      display: flex;
      align-items: center;
      justify-content: center;
      border-radius: 50%;
      border: 2px solid var(--a11y-border, #E0E0E0);
      color: var(--a11y-primary-text, white);
      font-weight: 600;
      text-transform: uppercase;
      user-select: none;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
    }

    .avatar-name {
      font-size: 16px;
      font-weight: 500;
      color: var(--a11y-text, #212121);
      max-width: 100px;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AvatarComponent {
  /** Inicial a mostrar (si no se proporciona, se calcula del nombre) */
  @Input() initial: string = '';

  /** Color de fondo del avatar en formato hexadecimal */
  @Input() color: string = 'var(--a11y-primary, #2196F3)';

  /** Tamanio del avatar en pixeles */
  @Input() size: number = 80;

  /** Nombre completo del usuario */
  @Input() name: string = '';

  /** Indica si el avatar esta seleccionado */
  @Input() selected: boolean = false;

  /** Mostrar el nombre debajo del avatar */
  @Input() showName: boolean = true;

  /** Etiqueta para accesibilidad */
  @Input() ariaLabel: string = '';

  /** Evento emitido al hacer click */
  @Output() avatarClick = new EventEmitter<void>();

  /** Tamanio de fuente calculado segun el tamanio del avatar */
  get fontSize(): number {
    return Math.round(this.size * 0.4);
  }

  /** Inicial calculada del nombre si no se proporciona */
  get displayInitial(): string {
    if (this.initial) {
      return this.initial.charAt(0).toUpperCase();
    }
    if (this.name) {
      return this.name.charAt(0).toUpperCase();
    }
    return '?';
  }

  handleClick(): void {
    this.avatarClick.emit();
  }

  ngOnInit(): void {
    if (!this.initial && this.name) {
      this.initial = this.name.charAt(0).toUpperCase();
    }
  }
}
