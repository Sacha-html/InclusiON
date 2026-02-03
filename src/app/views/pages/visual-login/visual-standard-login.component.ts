import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { VisualStandardLoginRequest } from '../../../models';
import { BaseVisualLoginComponent } from './base-visual-login.component';
import { AccessibilityPanelComponent } from '../../../components/accessibility-panel/accessibility-panel.component';
import {
  ButtonDirective,
  FormControlDirective,
  InputGroupComponent,
  InputGroupTextDirective,
  SpinnerComponent,
  FormCheckComponent,
  FormCheckInputDirective,
  FormCheckLabelDirective,
} from '@coreui/angular';
import { IconDirective } from '@coreui/icons-angular';

@Component({
  selector: 'app-visual-standard-login',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonDirective,
    FormControlDirective,
    InputGroupComponent,
    InputGroupTextDirective,
    SpinnerComponent,
    FormCheckComponent,
    FormCheckInputDirective,
    FormCheckLabelDirective,
    IconDirective,
    AccessibilityPanelComponent,
  ],
  templateUrl: './visual-standard-login.component.html',
  styleUrl: './visual-standard-login.component.scss',
})
export class VisualStandardLoginComponent extends BaseVisualLoginComponent {
  // Estado específico de contraseña
  password = '';
  showPassword = false;
  rememberDevice = false;

  // ============================================
  // Submit (implementación requerida)
  // ============================================

  onSubmit(): void {
    if (!this.password || this.isLoading || this.isLocked) return;

    this.isLoading = true;
    this.clearError();

    const request: VisualStandardLoginRequest = {
      userId: this.userId,
      password: this.password,
      deviceId: this.authService.getDeviceId(),
      rememberDevice: this.rememberDevice,
    };

    this.authService.loginVisualStandard(request).subscribe({
      next: (response) => {
        if (response.success && response.data?.success) {
          this.navigateToDashboard();
        } else {
          this.handleLoginResponseError(
            response.data,
            'Contraseña incorrecta',
            () => this.password = ''
          );
        }
      },
      error: (error) => {
        this.handleHttpError(
          error,
          'Error al verificar la contraseña',
          () => this.password = ''
        );
      },
    });
  }

  // ============================================
  // UI Helpers
  // ============================================

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }
}
