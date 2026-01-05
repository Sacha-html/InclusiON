import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { ProfileSelectLoginRequest } from '../../../models';
import { AccessibilityPanelComponent } from '../../../components/accessibility-panel/accessibility-panel.component';
import { ButtonDirective } from '@coreui/angular';
import { IconDirective } from '@coreui/icons-angular';

@Component({
  selector: 'app-profile-select-login',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonDirective,
    IconDirective,
    AccessibilityPanelComponent,
  ],
  templateUrl: './profile-select-login.component.html',
  styleUrl: './profile-select-login.component.scss',
})
export class ProfileSelectLoginComponent implements OnInit {
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private authService = inject(AuthService);

  userId = '';
  displayName = '';
  initial = '';
  avatarColor = '#667eea';

  isLoading = false;
  errorMessage = '';
  isConfirming = false;
  confirmationPin = '';
  requiresConfirmation = false;

  ngOnInit(): void {
    const params = this.route.snapshot.queryParams;
    this.userId = params['userId'] || '';
    this.displayName = params['displayName'] || '';
    this.initial = params['initial'] || this.displayName.charAt(0).toUpperCase();
    this.avatarColor = params['avatarColor'] || '#667eea';
    this.requiresConfirmation = params['requiresConfirmation'] === 'true';

    if (!this.userId) {
      this.router.navigate(['/login']);
    }
  }

  onSelectProfile(): void {
    if (this.requiresConfirmation) {
      this.isConfirming = true;
      return;
    }

    this.doLogin();
  }

  onConfirmWithPin(): void {
    if (!this.confirmationPin || this.confirmationPin.length < 4) {
      this.errorMessage = 'Por favor, ingresa un PIN válido';
      return;
    }

    this.doLogin();
  }

  private doLogin(): void {
    this.isLoading = true;
    this.errorMessage = '';

    const request: ProfileSelectLoginRequest = {
      userId: parseInt(this.userId, 10),
      deviceId: this.authService.getDeviceId(),
      requiresConfirmation: this.requiresConfirmation,
      confirmationPin: this.confirmationPin || undefined,
    };

    this.authService.loginWithProfileSelect(request).subscribe({
      next: (response) => {
        if (response.success && response.data?.success) {
          this.router.navigate(['/dashboard']);
        } else {
          this.errorMessage = response.data?.errorMessage || 'No se pudo iniciar sesión';
          this.isLoading = false;
          this.confirmationPin = '';
        }
      },
      error: (error) => {
        console.error('Profile select login error:', error);
        this.errorMessage = error.message || 'Error al iniciar sesión';
        this.isLoading = false;
        this.confirmationPin = '';
      },
    });
  }

  cancelConfirmation(): void {
    this.isConfirming = false;
    this.confirmationPin = '';
    this.errorMessage = '';
  }

  goBack(): void {
    this.router.navigate(['/login/identify'], {
      queryParams: { userType: 'PERSON' },
    });
  }

  goToRoleSelection(): void {
    this.router.navigate(['/login']);
  }
}
