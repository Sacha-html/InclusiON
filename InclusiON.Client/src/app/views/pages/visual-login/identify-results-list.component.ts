import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { UserMatchSummary } from '@models';

/**
 * Lista visual de candidatos cuando el identifier matchea más de un usuario.
 * Cada card muestra el avatar (con su color asignado) + inicial + nombre + inicial apellido,
 * pensado para que la persona reconozca rápidamente cuál es ella.
 */
@Component({
  selector: 'app-identify-results-list',
  standalone: true,
  imports: [],
  templateUrl: './identify-results-list.component.html',
  styleUrl: './identify-results-list.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class IdentifyResultsListComponent {
  @Input({ required: true }) matches: UserMatchSummary[] = [];
  @Input() ariaLabel = 'Tocá tu cara para entrar';

  @Output() select = new EventEmitter<UserMatchSummary>();
}
