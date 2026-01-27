import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { AssistedLoginRequest } from '../../../models';
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
export class AssistedLoginComponent implements OnInit {
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private authService = inject(AuthService);

  // Datos de la persona que necesita ayuda
  userId = '';
  displayName = '';
  initial = '';
  avatarColor = '#667eea';

  // Credenciales del supervisor
  supervisorEmail = '';
  supervisorPassword = '';
  showPassword = false;

  isLoading = false;
  errorMessage = '';
  isLocked = false;

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
    if (!this.supervisorEmail || !this.supervisorPassword || this.isLoading) return;

    this.isLoading = true;
    this.errorMessage = '';

    const request: AssistedLoginRequest = {
      userId: this.userId,
      supervisorEmail: this.supervisorEmail.trim(),
      supervisorPassword: this.supervisorPassword,
      deviceId: this.authService.getDeviceId(),
    };

    this.authService.loginAssisted(request).subscribe({
      next: (response) => {
        if (response.success && response.data?.success) {
          this.router.navigate(['/dashboard']);
        } else {
          this.handleLoginError(response.data);
        }
      },
      error: (error) => {
        console.error('Assisted login error:', error);
        this.errorMessage = error.message || 'Error al autorizar el acceso';
        this.supervisorPassword = '';
        this.isLoading = false;
      },
    });
  }

  private handleLoginError(data: any): void {
    this.supervisorPassword = '';
    this.isLoading = false;

    if (data?.isLocked) {
      this.isLocked = true;
      this.errorMessage = 'Cuenta del supervisor bloqueada por intentos fallidos';
    } else {
      this.errorMessage = data?.errorMessage || 'Credenciales del supervisor invalidas o no autorizado';
    }
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
