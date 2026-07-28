import { Component, Input } from '@angular/core';
import { IconDirective } from '@coreui/icons-angular';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  imports: [IconDirective],
  templateUrl: './empty-state.component.html',
})
export class EmptyStateComponent {
  @Input() icon    = 'cilNotes';
  @Input() message = 'Sin registros';
  @Input() detail  = '';
}
