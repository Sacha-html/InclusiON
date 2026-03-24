import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FamilyLoginRequest } from '@models';
import { BaseVisualLoginComponent } from './base-visual-login.component';
import { AccessibilityPanelComponent } from '@components/accessibility-panel/accessibility-panel.component';
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
  selector: 'app-family-login',
  standalone: true,
  imports: [
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
  templateUrl: './family-login.component.html',
  styleUrl: './family-login.component.scss',
})
export class FamilyLoginComponent extends BaseVisualLoginComponent {
  password = '';
  showPassword = false;
  rememberDevice = false;

  onSubmit(): void {
    if (!this.password || this.isLoading || this.isLocked) return;

    this.isLoading = true;
    this.clearError();

    const request: FamilyLoginRequest = {
      userId: this.userId,
      password: this.password,
      deviceId: this.authService.getDeviceId(),
      rememberDevice: this.rememberDevice,
    };

    this.authService.loginFamily(request).subscribe({
      next: (response) => {
        if (response.success && response.data?.success) {
          this.navigateToDashboard(response.data.mustChangePassword);
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

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  override goBack(): void {
    this.router.navigate(['/login/identify'], {
      queryParams: { userType: 'FAMILY' }
    });
  }
}
