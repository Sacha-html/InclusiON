import { Component, Input } from '@angular/core';
import { IconDirective } from '@coreui/icons-angular';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  imports: [IconDirective],
  template: `
    <div class="text-center text-body-secondary py-5">
      @if (icon) {
        <div><svg cIcon [name]="icon" size="3xl" class="mb-3 opacity-50"></svg></div>
      }
      <p class="mb-1 fw-medium">{{ message }}</p>
      @if (detail) {
        <small>{{ detail }}</small>
      }
    </div>
  `,
})
export class EmptyStateComponent {
  @Input() icon    = 'cilNotes';
  @Input() message = 'Sin registros';
  @Input() detail  = '';
}
