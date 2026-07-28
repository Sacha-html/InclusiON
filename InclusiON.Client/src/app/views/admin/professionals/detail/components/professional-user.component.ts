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
  templateUrl: './professional-user.component.html',
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
