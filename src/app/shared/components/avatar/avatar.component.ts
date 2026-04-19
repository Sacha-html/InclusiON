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
  templateUrl: './avatar.component.html',
  styleUrl: './avatar.component.scss',
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
