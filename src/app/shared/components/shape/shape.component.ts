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
  templateUrl: './shape.component.html',
  styleUrl: './shape.component.scss',
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
