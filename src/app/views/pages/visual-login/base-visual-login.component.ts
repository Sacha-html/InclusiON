import { inject, OnInit, OnDestroy, Directive } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService, ErrorCodeService } from '@services';
import { AppRoutes } from '@shared/constants/app-routes';
import { ErrorCode } from '@models';
import { RoleRoutes } from '@shared/constants/roles';

/**
 * Datos de error de respuesta de login visual.
 */
export interface LoginErrorResponse {
  errorCode?: number;
  userMessage?: string;
  isLocked?: boolean;
  lockoutSecondsRemaining?: number;
  remainingAttempts?: number;
  errorMessage?: string;
}

/**
 * Datos del usuario obtenidos de los query params
 */
export interface VisualLoginUserData {
  userId: string;
  displayName: string;
  initial: string;
  avatarColor: string;
}

/**
 * Clase base abstracta para componentes de login visual.
 * Centraliza la lógica común de:
 * - Carga de datos del usuario desde query params
 * - Estado de UI (loading, error messages)
 * - Manejo de bloqueo de cuenta (lockout)
 * - Navegación
 * - Manejo de errores con ErrorCodeService
 *
 * @usageNotes
 * Extender esta clase e implementar el método onSubmit():
 * ```typescript
 * export class PinLoginComponent extends BaseVisualLoginComponent {
 *   onSubmit(): void {
 *     // Lógica específica de login
 *   }
 * }
 * ```
 */
@Directive()
export abstract class BaseVisualLoginComponent implements OnInit, OnDestroy {
  // ============================================
  // Servicios protegidos (accesibles por hijos)
  // ============================================
  protected router = inject(Router);
  protected route = inject(ActivatedRoute);
  protected authService = inject(AuthService);
  protected errorCodeService = inject(ErrorCodeService);

  // ============================================
  // Datos del usuario (desde query params)
  // ============================================
  userId = '';
  displayName = '';
  initial = '';
  avatarColor = '#667eea';

  // ============================================
  // Estado de UI
  // ============================================
  isLoading = false;
  errorMessage = '';

  // ============================================
  // Estado de bloqueo (lockout)
  // ============================================
  isLocked = false;
  lockoutSeconds = 0;
  remainingAttempts: number | null = null;

  private lockoutIntervalId: ReturnType<typeof setInterval> | null = null;

  // ============================================
  // Lifecycle hooks
  // ============================================

  ngOnInit(): void {
    this.loadUserFromParams();
    this.onComponentInit();
  }

  ngOnDestroy(): void {
    this.clearLockoutTimer();
    this.onComponentDestroy();
  }

  /**
   * Hook para inicialización adicional en componentes hijos.
   * Se llama después de cargar los datos del usuario.
   */
  protected onComponentInit(): void {
    // Override en componentes hijos si es necesario
  }

  /**
   * Hook para limpieza adicional en componentes hijos.
   * Se llama antes de limpiar el timer de lockout.
   */
  protected onComponentDestroy(): void {
    // Override en componentes hijos si es necesario
  }

  // ============================================
  // Carga de datos del usuario
  // ============================================

  /**
   * Carga los datos del usuario desde los query params.
   * Redirige a /login si no hay userId.
   */
  protected loadUserFromParams(): void {
    const params = this.route.snapshot.queryParams;

    this.userId = params['userId'] || '';
    this.displayName = params['displayName'] || '';
    this.initial = params['initial'] || this.getInitialFromName(this.displayName);
    this.avatarColor = params['avatarColor'] || '#667eea';

    if (!this.userId) {
      this.router.navigate([AppRoutes.Login]);
    }
  }

  /**
   * Obtiene la inicial del nombre para el avatar.
   */
  private getInitialFromName(name: string): string {
    return name.length > 0 ? name.charAt(0).toUpperCase() : '?';
  }

  /**
   * Obtiene los datos del usuario como objeto.
   */
  protected getUserData(): VisualLoginUserData {
    return {
      userId: this.userId,
      displayName: this.displayName,
      initial: this.initial,
      avatarColor: this.avatarColor
    };
  }

  // ============================================
  // Manejo de bloqueo (lockout)
  // ============================================

  /**
   * Inicia el temporizador de cuenta regresiva para el bloqueo.
   */
  protected startLockoutTimer(): void {
    this.clearLockoutTimer();

    this.lockoutIntervalId = setInterval(() => {
      this.lockoutSeconds--;

      if (this.lockoutSeconds <= 0) {
        this.clearLockoutTimer();
        this.isLocked = false;
        this.errorMessage = '';
      }
    }, 1000);
  }

  /**
   * Limpia el temporizador de bloqueo.
   */
  protected clearLockoutTimer(): void {
    if (this.lockoutIntervalId !== null) {
      clearInterval(this.lockoutIntervalId);
      this.lockoutIntervalId = null;
    }
  }

  /**
   * Activa el estado de bloqueo con los segundos especificados.
   */
  protected activateLockout(seconds: number = 60): void {
    this.isLocked = true;
    this.lockoutSeconds = seconds;
    this.startLockoutTimer();
    this.errorMessage = `Cuenta bloqueada. Espera ${seconds} segundos.`;
  }

  // ============================================
  // Manejo de errores
  // ============================================

  /**
   * Maneja errores HTTP del interceptor (con errorCode enriquecido).
   * @param error El error del interceptor
   * @param defaultMessage Mensaje por defecto si no hay errorCode
   * @param clearFieldFn Función opcional para limpiar campos del form
   */
  protected handleHttpError(
    error: LoginErrorResponse,
    defaultMessage: string,
    clearFieldFn?: () => void
  ): void {
    clearFieldFn?.();
    this.isLoading = false;

    if (error.errorCode !== undefined) {
      this.errorMessage = this.errorCodeService.getMessage(error.errorCode);

      if (error.errorCode === ErrorCode.AccountLocked) {
        this.activateLockout(60);
      }
    } else {
      this.errorMessage = error.userMessage || defaultMessage;
    }
  }

  /**
   * Maneja errores de respuesta de login (data.success = false).
   * @param data Los datos de la respuesta
   * @param defaultMessage Mensaje por defecto
   * @param clearFieldFn Función opcional para limpiar campos del form
   */
  protected handleLoginResponseError(
    data: LoginErrorResponse,
    defaultMessage: string,
    clearFieldFn?: () => void
  ): void {
    clearFieldFn?.();
    this.isLoading = false;

    if (data?.isLocked) {
      const seconds = data.lockoutSecondsRemaining || 60;
      this.activateLockout(seconds);
    } else {
      this.remainingAttempts = data?.remainingAttempts ?? null;
      this.errorMessage = data?.errorMessage || defaultMessage;

      // Agregar advertencia si quedan pocos intentos
      if (this.remainingAttempts !== null && this.remainingAttempts <= 2) {
        this.errorMessage += `. Te quedan ${this.remainingAttempts} intentos.`;
      }
    }
  }

  /**
   * Limpia el mensaje de error.
   */
  protected clearError(): void {
    this.errorMessage = '';
  }

  // ============================================
  // Navegación
  // ============================================

  /**
   * Navega de vuelta a la pantalla de identificación.
   */
  goBack(): void {
    this.router.navigate(['/login/identify'], {
      queryParams: { userType: 'PERSON' }
    });
  }

  /**
   * Navega al dashboard después de login exitoso.
   * Si mustChangePassword es true, redirige a cambio de contraseña.
   */
  protected navigateToDashboard(mustChangePassword?: boolean): void {
    if (mustChangePassword) {
      this.router.navigate([AppRoutes.ChangePassword]);
    } else {
      const role = this.authService.getUserRole();
      const target = role ? (RoleRoutes[role] || '/') : '/';
      this.router.navigate([target]);
    }
  }

  // ============================================
  // Métodos abstractos (deben implementar los hijos)
  // ============================================

  /**
   * Método de envío del formulario.
   * Debe ser implementado por cada componente hijo.
   */
  abstract onSubmit(): void;
}
