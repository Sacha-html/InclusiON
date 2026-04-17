import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';

export type ShapeType = 'circle' | 'square' | 'triangle' | 'star';

/**
 * Componente para renderizar formas geometricas para login visual.
 * Soporta: circulo, cuadrado, triangulo y estrella.
 * Accesible: incluye aria-label y soporte para teclado.
 */
@Component({
  selector: 'app-shape',
  standalone: true,
  imports: [],
  template: `
    <button
      type="button"
      class="shape-button"
      [class.selected]="selected"
      [attr.aria-label]="ariaLabel"
      [attr.aria-pressed]="selected"
      (click)="handleClick()"
      (keydown.enter)="handleClick()"
      (keydown.space)="handleClick()">
      <svg
        [attr.width]="size"
        [attr.height]="size"
        [attr.viewBox]="'0 0 ' + size + ' ' + size"
        aria-hidden="true"
        focusable="false">
        @switch (type) {
          @case ('circle') {
            <circle
              [attr.cx]="size / 2"
              [attr.cy]="size / 2"
              [attr.r]="size / 2 - strokeWidth"
              [attr.fill]="color"
              [attr.stroke]="selected ? strokeColor : 'none'"
              [attr.stroke-width]="selected ? strokeWidth : 0" />
          }
          @case ('square') {
            <rect
              [attr.x]="strokeWidth"
              [attr.y]="strokeWidth"
              [attr.width]="size - strokeWidth * 2"
              [attr.height]="size - strokeWidth * 2"
              [attr.fill]="color"
              [attr.stroke]="selected ? strokeColor : 'none'"
              [attr.stroke-width]="selected ? strokeWidth : 0" />
          }
          @case ('triangle') {
            <polygon
              [attr.points]="trianglePoints"
              [attr.fill]="color"
              [attr.stroke]="selected ? strokeColor : 'none'"
              [attr.stroke-width]="selected ? strokeWidth : 0" />
          }
          @case ('star') {
            <polygon
              [attr.points]="starPoints"
              [attr.fill]="color"
              [attr.stroke]="selected ? strokeColor : 'none'"
              [attr.stroke-width]="selected ? strokeWidth : 0" />
          }
        }
      </svg>
    </button>
  `,
  styles: [`
    .shape-button {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      padding: 8px;
      border: 2px solid transparent;
      border-radius: 8px;
      background: transparent;
      cursor: pointer;
      transition: all 0.2s ease;
      min-width: 44px;
      min-height: 44px;
    }

    @media (prefers-reduced-motion: reduce) {
      .shape-button {
        transition: none;
      }
      .shape-button:hover,
      .shape-button:active {
        transform: none;
      }
    }

    .shape-button:hover {
      background-color: rgba(0, 0, 0, 0.05);
      transform: scale(1.05);
    }

    .shape-button:focus-visible {
      outline: 3px solid var(--a11y-focus-accent, #0D47A1);
      outline-offset: 2px;
    }

    .shape-button.selected {
      border-color: var(--a11y-primary, #0066CC);
      background-color: color-mix(in srgb, var(--a11y-primary, #0066CC) 10%, transparent);
    }

    .shape-button:active {
      transform: scale(0.95);
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShapeComponent {
  /** Tipo de forma a renderizar */
  @Input() type: ShapeType = 'circle';

  /** Color de la forma en formato hexadecimal */
  @Input() color: string = 'var(--a11y-danger, #F44336)';

  /** Tamanio en pixeles */
  @Input() size: number = 64;

  /** Indica si la forma esta seleccionada */
  @Input() selected: boolean = false;

  /** Etiqueta para accesibilidad */
  @Input() ariaLabel: string = 'Forma';

  /** Evento emitido al hacer click */
  @Output() shapeClick = new EventEmitter<void>();

  /** Ancho del borde de seleccion */
  readonly strokeWidth = 4;

  /** Color del borde de seleccion, resuelto desde CSS variable */
  get strokeColor(): string {
    return getComputedStyle(document.documentElement).getPropertyValue('--a11y-text').trim() || '#000';
  }

  /** Puntos del triangulo */
  get trianglePoints(): string {
    const padding = this.strokeWidth;
    const width = this.size - padding * 2;
    const height = this.size - padding * 2;
    return `${this.size / 2},${padding} ${padding},${height + padding} ${width + padding},${height + padding}`;
  }

  /** Puntos de la estrella (5 puntas) */
  get starPoints(): string {
    const cx = this.size / 2;
    const cy = this.size / 2;
    const outerR = this.size / 2 - this.strokeWidth;
    const innerR = outerR / 2.5;
    const points = 5;
    const angle = Math.PI / points;
    const coords: string[] = [];

    for (let i = 0; i < 2 * points; i++) {
      const r = i % 2 === 0 ? outerR : innerR;
      const x = cx + r * Math.sin(i * angle);
      const y = cy - r * Math.cos(i * angle);
      coords.push(`${x},${y}`);
    }

    return coords.join(' ');
  }

  handleClick(): void {
    this.shapeClick.emit();
  }
}
