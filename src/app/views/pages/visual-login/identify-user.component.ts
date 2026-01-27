import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { IdentifyUserRequest, IdentifyUserData } from '../../../models';
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
  selector: 'app-identify-user',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
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
  templateUrl: './identify-user.component.html',
  styleUrl: './identify-user.component.scss',
})
export class IdentifyUserComponent implements OnInit {
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private authService = inject(AuthService);

  userType: 'PERSON' | 'PROFESSIONAL' | 'FAMILY' = 'PERSON';
  identifier = '';
  isLoading = false;
  errorMessage = '';

  userTypeLabels: Record<string, string> = {
    PERSON: 'Persona',
    PROFESSIONAL: 'Profesional',
    FAMILY: 'Familiar',
  };

  ngOnInit(): void {
    const type = this.route.snapshot.queryParams['userType'];
    if (type && ['PERSON', 'PROFESSIONAL', 'FAMILY'].includes(type)) {
      this.userType = type;
    }
  }

  get userTypeLabel(): string {
    return this.userTypeLabels[this.userType] || 'Usuario';
  }

  get placeholderText(): string {
    switch (this.userType) {
      case 'PERSON':
        return 'Escribe tu nombre...';
      case 'PROFESSIONAL':
        return 'Escribe tu nombre o email...';
      case 'FAMILY':
        return 'Escribe tu nombre...';
      default:
        return 'Escribe tu nombre...';
    }
  }

  onIdentify(): void {
    if (!this.identifier.trim()) {
      this.errorMessage = 'Por favor, escribe tu nombre';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const request: IdentifyUserRequest = {
      identifier: this.identifier.trim(),
      deviceId: this.authService.getDeviceId(),
      userType: this.userType,
    };

    this.authService.identifyUser(request).subscribe({
      next: (response) => {
        if (response.success && response.data?.userFound) {
          this.navigateToLoginMethod(response.data);
        } else {
          this.errorMessage =
            response.data?.errorMessage || 'No encontramos tu cuenta. Verifica tu nombre.';
          this.isLoading = false;
        }
      },
      error: (error) => {
        console.error('Error identifying user:', error);
        this.errorMessage = error.message || 'Ocurrió un error. Intenta de nuevo.';
        this.isLoading = false;
      },
    });
  }

  private navigateToLoginMethod(userData: IdentifyUserData): void {
    const baseParams = {
      userId: userData.userId,
      displayName: userData.displayName,
      initial: userData.initial,
      avatarColor: userData.avatarColor,
    };

    // Si el metodo esta deprecado, mostrar error
    if (userData.loginMethodCode === 'DEPRECATED') {
      this.errorMessage = userData.errorMessage || 'Tu metodo de acceso necesita actualizarse. Contacta a un administrador.';
      this.isLoading = false;
      return;
    }

    // Navigate based on login method
    switch (userData.loginMethodCode) {
      case 'STANDARD':
        this.router.navigate(['/login/standard'], { queryParams: baseParams });
        break;
      case 'PIN':
        this.router.navigate(['/login/pin'], { queryParams: baseParams });
        break;
      case 'SUPERVISED':
      case 'ASSISTED':
        this.router.navigate(['/login/assisted'], { queryParams: baseParams });
        break;
      default:
        // Para metodos desconocidos, mostrar error amigable
        this.errorMessage = 'Tu metodo de acceso no esta disponible. Contacta a un administrador.';
        this.isLoading = false;
    }
  }

  goBack(): void {
    this.router.navigate(['/login']);
  }

  clearInput(): void {
    this.identifier = '';
    this.errorMessage = '';
  }
}
