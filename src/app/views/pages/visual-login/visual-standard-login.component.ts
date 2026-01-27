import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { VisualStandardLoginRequest } from '../../../models';
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
    FormCheckComponent,
    FormCheckInputDirective,
    FormCheckLabelDirective,
    IconDirective,
    AccessibilityPanelComponent,
  ],
  templateUrl: './visual-standard-login.component.html',
  styleUrl: './visual-standard-login.component.scss',
})
export class VisualStandardLoginComponent implements OnInit {
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private authService = inject(AuthService);

  userId = '';
  displayName = '';
  initial = '';
  avatarColor = '#667eea';

  password = '';
  showPassword = false;
  isLoading = false;
  errorMessage = '';
  remainingAttempts: number | null = null;
  isLocked = false;
  lockoutSeconds = 0;
  rememberDevice = false;

  ngOnInit(): void {
    const params = this.route.snapshot.queryParams;
    this.userId = params['userId'] || '';
    this.displayName = params['displayName'] || '';
    this.initial = params['initial'] || this.displayName.charAt(0).toUpperCase();
    this.avatarColor = params['avatarColor'] || '#667eea';

    if (!this.userId) {
      this.router.navigate(['/login']);
    }
  }

  onSubmit(): void {
    if (!this.password || this.isLoading || this.isLocked) return;

    this.isLoading = true;
    this.errorMessage = '';

    const request: VisualStandardLoginRequest = {
      userId: this.userId,
      password: this.password,
      deviceId: this.authService.getDeviceId(),
      rememberDevice: this.rememberDevice,
    };

    this.authService.loginVisualStandard(request).subscribe({
      next: (response) => {
        if (response.success && response.data?.success) {
          this.router.navigate(['/dashboard']);
        } else {
          this.handleLoginError(response.data);
        }
      },
      error: (error) => {
        console.error('Visual standard login error:', error);
        this.errorMessage = error.message || 'Error al verificar la contrasena';
        this.password = '';
        this.isLoading = false;
      },
    });
  }

  private handleLoginError(data: any): void {
    this.password = '';
    this.isLoading = false;

    if (data?.isLocked) {
      this.isLocked = true;
      this.lockoutSeconds = data.lockoutSecondsRemaining || 60;
      this.startLockoutTimer();
      this.errorMessage = `Cuenta bloqueada. Espera ${this.lockoutSeconds} segundos.`;
    } else {
      this.remainingAttempts = data?.remainingAttempts || null;
      this.errorMessage = data?.errorMessage || 'Contrasena incorrecta';

      if (this.remainingAttempts !== null && this.remainingAttempts <= 2) {
        this.errorMessage += `. Te quedan ${this.remainingAttempts} intentos.`;
      }
    }
  }

  private startLockoutTimer(): void {
    const interval = setInterval(() => {
      this.lockoutSeconds--;
      if (this.lockoutSeconds <= 0) {
        clearInterval(interval);
        this.isLocked = false;
        this.errorMessage = '';
      }
    }, 1000);
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  goBack(): void {
    this.router.navigate(['/login/identify'], {
      queryParams: { userType: 'PERSON' },
    });
  }
}
