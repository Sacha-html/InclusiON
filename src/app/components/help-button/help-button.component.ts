import { Component, inject } from '@angular/core';
import { IconDirective } from '@coreui/icons-angular';
import { ToastService } from '@services';

@Component({
  selector: 'app-help-button',
  standalone: true,
  imports: [IconDirective],
  templateUrl: './help-button.component.html',
  styleUrl: './help-button.component.scss',
})
export class HelpButtonComponent {
  private readonly toastService = inject(ToastService);

  requestHelp(): void {
    this.toastService.info('Ayuda solicitada. Tu cuidador fue notificado.');
  }
}
