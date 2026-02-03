import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AssistedLoginRequest, ErrorCode } from '../../../models';
import { BaseVisualLoginComponent } from './base-visual-login.component';
import { AccessibilityPanelComponent } from '../../../components/accessibility-panel/accessibility-panel.component';
import {
  ContainerComponent,
  RowComponent,
  ColComponent,
  CardComponent,
  CardBodyComponent,
  ButtonDirective,
  FormControlDirective,
  InputGroupComponent,
  InputGroupTextDirective,
  SpinnerComponent,
  AlertComponent,
} from '@coreui/angular';
import { IconDirective } from '@coreui/icons-angular';

@Component({
  selector: 'app-assisted-login',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ContainerComponent,
    RowComponent,
    ColComponent,
    CardComponent,
    CardBodyComponent,
    ButtonDirective,
    FormControlDirective,
    InputGroupComponent,
    InputGroupTextDirective,
    SpinnerComponent,
    AlertComponent,
    IconDirective,
    AccessibilityPanelComponent,
  ],
  templateUrl: './assisted-login.component.html',
  styleUrl: './assisted-login.component.scss',
})
export class AssistedLoginComponent extends BaseVisualLoginComponent {
  // Credenciales del supervisor
  supervisorEmail = '';
  supervisorPassword = '';
  showPassword = false;

  // ============================================
  // Submit (implementación requerida)
  // ============================================

  onSubmit(): void {
    if (!this.supervisorEmail || !this.supervisorPassword || this.isLoading) return;

    this.isLoading = true;
    this.clearError();

    const request: AssistedLoginRequest = {
      userId: this.userId,
      supervisorEmail: this.supervisorEmail.trim(),
      supervisorPassword: this.supervisorPassword,
      deviceId: this.authService.getDeviceId(),
    };

    this.authService.loginAssisted(request).subscribe({
      next: (response) => {
        if (response.success && response.data?.success) {
          this.navigateToDashboard();
        } else {
          this.handleAssistedLoginError(response.data);
        }
      },
      error: (error) => {
        this.handleHttpError(
          error,
          'Error al autorizar el acceso',
          () => this.supervisorPassword = ''
        );
      },
    });
  }

  /**
   * Maneja errores específicos del login asistido.
   * No usa lockout timer ya que el bloqueo es del supervisor.
   */
  private handleAssistedLoginError(data: any): void {
    this.supervisorPassword = '';
    this.isLoading = false;

    if (data?.isLocked) {
      this.isLocked = true;
      this.errorMessage = this.errorCodeService.getMessage(ErrorCode.AccountLocked);
    } else {
      this.errorMessage = data?.errorMessage
        || this.errorCodeService.getMessage(ErrorCode.SupervisorNotAuthorized);
    }
  }

  // ============================================
  // UI Helpers
  // ============================================

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }
}
