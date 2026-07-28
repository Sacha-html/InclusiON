import { Component, inject, signal } from '@angular/core';
import { IconDirective } from '@coreui/icons-angular';
import { ToastService, PersonsService } from '@services';

@Component({
  selector: 'app-help-button',
  standalone: true,
  imports: [IconDirective],
  templateUrl: './help-button.component.html',
  styleUrl: './help-button.component.scss',
})
export class HelpButtonComponent {
  private readonly toastService  = inject(ToastService);
  private readonly personsService = inject(PersonsService);

  readonly sending = signal(false);

  requestHelp(): void {
    if (this.sending()) return;

    this.sending.set(true);

    this.personsService.requestHelp().subscribe({
      next: (_) => {
        this.toastService.success('Tu cuidador fue notificado. ¡Pronto vendrán a ayudarte!');
      },
      error: () => {
        this.toastService.warning('No se pudo enviar la solicitud. Intentá de nuevo.');
      },
      complete: () => this.sending.set(false),
    });
  }
}
