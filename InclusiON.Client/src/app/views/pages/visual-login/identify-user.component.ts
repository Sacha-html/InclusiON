import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService, ErrorCodeService } from '@services';
import { IdentifyUserRequest, IdentifyUserData, UserMatchSummary } from '@models';
import { AccessibilityPanelComponent } from '@components/accessibility-panel/accessibility-panel.component';
import { ButtonDirective } from '@coreui/angular';
import { IconDirective } from '@coreui/icons-angular';
import { IdentifyResultsListComponent } from './identify-results-list.component';
import { UserRoles } from '@shared/constants/roles';
import { AppRoutes } from '@shared/constants/app-routes';

const MIN_IDENTIFIER_LENGTH = 3;

@Component({
  selector: 'app-identify-user',
  standalone: true,
  imports: [
    FormsModule,
    ButtonDirective,
    IconDirective,
    AccessibilityPanelComponent,
    IdentifyResultsListComponent,
  ],
  templateUrl: './identify-user.component.html',
  styleUrl: './identify-user.component.scss',
})
export class IdentifyUserComponent implements OnInit {
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private authService = inject(AuthService);
  private errorCodeService = inject(ErrorCodeService);

  userType: 'PERSON' | 'PROFESSIONAL' | 'FAMILY' = 'PERSON';
  identifier = '';
  isLoading = false;
  errorMessage = '';
  matches: UserMatchSummary[] | null = null;

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
        return 'Escribe tu email...';
      default:
        return 'Escribe tu nombre...';
    }
  }

  onIdentify(): void {
    const trimmed = this.identifier.trim();
    if (!trimmed) {
      this.errorMessage = this.userType === 'FAMILY' ? 'Por favor, escribe tu email' : 'Por favor, escribe tu nombre';
      return;
    }
    if (trimmed.length < MIN_IDENTIFIER_LENGTH) {
      this.errorMessage = `Escribe al menos ${MIN_IDENTIFIER_LENGTH} letras.`;
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';
    this.matches = null;

    const request: IdentifyUserRequest = {
      identifier: trimmed,
      deviceId: this.authService.getDeviceId(),
      userType: this.userType,
    };

    this.authService.identifyUser(request).subscribe({
      next: (response) => {
        if (response.success && response.data?.userFound) {
          if (response.data.requiresSelection && response.data.matches?.length) {
            this.matches = response.data.matches;
            this.isLoading = false;
            return;
          }
          this.navigateToLoginMethod(response.data);
        } else {
          this.errorMessage =
            response.data?.errorMessage || 'No encontramos tu cuenta. Verifica tu nombre.';
          this.isLoading = false;
        }
      },
      error: (error) => {
        this.isLoading = false;

        if (error.errorCode !== undefined) {
          this.errorMessage = this.errorCodeService.getMessage(error.errorCode);
        } else {
          this.errorMessage = error.userMessage || 'Ocurrió un error. Intenta de nuevo.';
        }
      },
    });
  }

  onMatchSelected(match: UserMatchSummary): void {
    this.navigateToLoginMethod({
      userFound: true,
      userId: match.userId,
      displayName: match.displayName + (match.lastNameInitial ? ' ' + match.lastNameInitial + '.' : ''),
      initial: match.initial,
      avatarColor: match.avatarColor,
      loginMethodCode: match.loginMethodCode,
      loginMethodName: match.loginMethodName,
      isTrustedDevice: match.isTrustedDevice,
      requiresSupervision: match.requiresSupervision,
      userType: UserRoles.Person,
    });
  }

  searchAgain(): void {
    this.matches = null;
    this.identifier = '';
    this.errorMessage = '';
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

    // Familiares siempre van a /login/family
    if (userData.userType === UserRoles.Family) {
      this.router.navigate([AppRoutes.LoginFamily], { queryParams: baseParams });
      return;
    }

    // Profesionales usan login con email+password
    if (userData.userType === UserRoles.Professional) {
      this.router.navigate([AppRoutes.AdminLogin]);
      return;
    }

    // Navigate based on login method
    switch (userData.loginMethodCode) {
      case 'STANDARD':
        this.router.navigate([AppRoutes.LoginStandard], { queryParams: baseParams });
        break;
      case 'PIN':
        this.router.navigate([AppRoutes.LoginPin], { queryParams: baseParams });
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
    this.router.navigate([AppRoutes.Login]);
  }

  clearInput(): void {
    this.identifier = '';
    this.errorMessage = '';
  }
}
