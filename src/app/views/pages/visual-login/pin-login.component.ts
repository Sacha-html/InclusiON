import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { PinLoginRequest } from '../../../models';
import { AccessibilityPanelComponent } from '../../../components/accessibility-panel/accessibility-panel.component';
import {
  ContainerComponent,
  RowComponent,
  ColComponent,
  CardComponent,
  CardBodyComponent,
  ButtonDirective,
  SpinnerComponent,
  AlertComponent,
  FormCheckComponent,
  FormCheckInputDirective,
  FormCheckLabelDirective,
} from '@coreui/angular';
import { IconDirective } from '@coreui/icons-angular';

@Component({
  selector: 'app-pin-login',
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
    SpinnerComponent,
    AlertComponent,
    FormCheckComponent,
    FormCheckInputDirective,
    FormCheckLabelDirective,
    IconDirective,
    AccessibilityPanelComponent,
  ],
  templateUrl: './pin-login.component.html',
  styleUrl: './pin-login.component.scss',
})
export class PinLoginComponent implements OnInit {
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private authService = inject(AuthService);

  userId = '';
  displayName = '';
  initial = '';
  avatarColor = '#667eea';

  pin = '';
  maxPinLength = 4;
  isLoading = false;
  errorMessage = '';
  remainingAttempts: number | null = null;
  isLocked = false;
  lockoutSeconds = 0;
  rememberDevice = false;

  pinPad = [
    ['1', '2', '3'],
    ['4', '5', '6'],
    ['7', '8', '9'],
    ['clear', '0', 'submit'],
  ];

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

  onPinDigit(digit: string): void {
    if (this.isLoading || this.isLocked) return;

    if (this.pin.length < this.maxPinLength) {
      this.pin += digit;
      this.errorMessage = '';

      // Auto-submit when 4 digits entered
      if (this.pin.length === this.maxPinLength) {
        this.onSubmit();
      }
    }
  }

  onClear(): void {
    this.pin = '';
    this.errorMessage = '';
  }

  onBackspace(): void {
    if (this.pin.length > 0) {
      this.pin = this.pin.slice(0, -1);
    }
  }

  onSubmit(): void {
    if (this.pin.length !== this.maxPinLength || this.isLoading) return;

    this.isLoading = true;
    this.errorMessage = '';

    const request: PinLoginRequest = {
      userId: this.userId,
      pin: this.pin,
      deviceId: this.authService.getDeviceId(),
      rememberDevice: this.rememberDevice,
    };

    this.authService.loginWithPin(request).subscribe({
      next: (response) => {
        if (response.success && response.data?.success) {
          this.router.navigate(['/dashboard']);
        } else {
          this.handleLoginError(response.data);
        }
      },
      error: (error) => {
        console.error('PIN login error:', error);
        this.errorMessage = error.message || 'Error al verificar el PIN';
        this.pin = '';
        this.isLoading = false;
      },
    });
  }

  private handleLoginError(data: any): void {
    this.pin = '';
    this.isLoading = false;

    if (data?.isLocked) {
      this.isLocked = true;
      this.lockoutSeconds = data.lockoutSecondsRemaining || 60;
      this.startLockoutTimer();
      this.errorMessage = `Cuenta bloqueada. Espera ${this.lockoutSeconds} segundos.`;
    } else {
      this.remainingAttempts = data?.remainingAttempts || null;
      this.errorMessage = data?.errorMessage || 'PIN incorrecto';

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

  get pinDots(): boolean[] {
    return Array(this.maxPinLength)
      .fill(false)
      .map((_, i) => i < this.pin.length);
  }

  goBack(): void {
    this.router.navigate(['/login/identify'], {
      queryParams: { userType: 'PERSON' },
    });
  }

  handlePadClick(key: string): void {
    if (key === 'clear') {
      this.onBackspace();
    } else if (key === 'submit') {
      this.onSubmit();
    } else {
      this.onPinDigit(key);
    }
  }

  getPadKeyLabel(key: string): string {
    if (key === 'clear') return '⌫';
    if (key === 'submit') return '✓';
    return key;
  }

  getPadKeyAriaLabel(key: string): string {
    if (key === 'clear') return 'Borrar último dígito';
    if (key === 'submit') return 'Confirmar PIN';
    return `Dígito ${key}`;
  }
}
