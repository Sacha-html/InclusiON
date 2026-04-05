import { Component, Input, Output, EventEmitter, inject } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService, ToastService, UserManagementService } from '@services';
import { ProfessionalResponse } from '@models';
import { AlertComponent, ButtonDirective, ModalComponent, ModalBodyComponent, ModalFooterComponent, ModalHeaderComponent, BadgeModule } from '@coreui/angular';
import { IconModule } from '@coreui/icons-angular';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-professional-user',
  standalone: true,
  imports: [
    FormsModule,
    AlertComponent,
    ButtonDirective,
    ModalComponent,
    ModalBodyComponent,
    ModalFooterComponent,
    ModalHeaderComponent,
    IconModule,
    BadgeModule,
    DatePipe,
  ],
  template: `
    @if (professional) {
      <div class="row">
        <div class="col-md-6">
          <div class="mb-3">
            <label class="form-label text-muted small">Email</label>
            <p class="mb-1">{{ professional.email || 'No asignado' }}</p>
          </div>
          <div class="mb-3">
            <label class="form-label text-muted small">Usuario desde</label>
            <p class="mb-1">{{ professional.createdAt | date:'dd/MM/yyyy' }}</p>
          </div>
        </div>
        <div class="col-md-6">
          <div class="mb-3">
            <label class="form-label text-muted small">Último acceso</label>
            <p class="mb-1">—</p>
          </div>
          <div class="mb-3">
            <label class="form-label text-muted small">Debe cambiar contraseña</label>
            <p class="mb-0">
              @if (professional.temporaryPassword) {
                <c-badge color="warning">Sí</c-badge>
              } @else {
                <c-badge color="success">No</c-badge>
              }
            </p>
          </div>
        </div>
      </div>

      <div class="mt-3">
        <button cButton color="primary" (click)="resetPassword()">
          <svg cIcon name="cilReload" class="me-2"></svg>
          Resetear contraseña
        </button>
      </div>

      <!-- Modal password -->
      <c-modal [visible]="showPasswordModal" (visibleChange)="showPasswordModal = $event">
        <c-modal-header>
          <h5 cModalTitle>Contraseña temporal generada</h5>
        </c-modal-header>
        <c-modal-body>
          <p>Se generó una contraseña temporal para <strong>{{ tempPasswordEmail }}</strong>.</p>
          <p>El usuario deberá cambiarla en su próximo inicio de sesión.</p>
          <c-alert color="warning" class="d-flex align-items-center justify-content-between">
            <code class="fs-5">{{ tempPassword }}</code>
            <button cButton color="primary" size="sm" (click)="copyPassword()">Copiar</button>
          </c-alert>
        </c-modal-body>
        <c-modal-footer>
          <button cButton color="secondary" (click)="showPasswordModal = false">Cerrar</button>
        </c-modal-footer>
      </c-modal>
    }
  `
})
export class ProfessionalUserComponent {
  private readonly userService = inject(UserManagementService);
  private readonly toastService = inject(ToastService);

  @Input() professional: ProfessionalResponse | null = null;

  showPasswordModal = false;
  tempPassword = '';
  tempPasswordEmail = '';

  resetPassword(): void {
    if (!this.professional?.userId) return;

    this.userService.resetPassword(this.professional.userId).subscribe({
      next: (result) => {
        this.tempPassword = result.temporaryPassword;
        this.tempPasswordEmail = result.userEmail;
        this.showPasswordModal = true;
        this.toastService.success('Contraseña reseteada exitosamente');
      },
      error: () => {
        this.toastService.error('Error al resetear la contraseña');
      },
    });
  }

  copyPassword(): void {
    navigator.clipboard.writeText(this.tempPassword).then(() => {
      this.toastService.success('Contraseña copiada al portapapeles');
    });
  }
}
