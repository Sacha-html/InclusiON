import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService, ErrorCodeService, ToastService, LocalStorageService } from '@services';
import { ErrorCode } from '@models';
import { RoleRoutes } from '@shared/constants/roles';

/** URLs públicas que no requieren autenticación */
const PUBLIC_URLS = ['/admin-login', '/register', '/visual-login', '/invite'];

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const authService = inject(AuthService);
  const errorCodeService = inject(ErrorCodeService);
  const toastService = inject(ToastService);
  const storageService = inject(LocalStorageService);

  const token = storageService.getAccessToken();

  let authRequest = req;

  if (token) {
    authRequest = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`,
      },
    });
  }

  return next(authRequest).pipe(
    catchError((error: HttpErrorResponse) => {
      // Extraer errorCode de la respuesta del backend si existe
      const errorCode: ErrorCode | undefined = error.error?.errorCode;
      const backendMessage: string | undefined = error.error?.message;

      // Manejar según el código de error del backend o HTTP status
      if (errorCode !== undefined) {
        handleErrorCode(errorCode, errorCodeService, toastService, router, storageService, authService);
      } else {
        handleHttpStatus(error.status, router, toastService, storageService, authService);
      }

      // Enriquecer el error con información adicional
      const enrichedError = {
        ...error,
        errorCode,
        errorInfo: errorCode !== undefined
          ? errorCodeService.getErrorInfo(errorCode)
          : undefined,
        userMessage: backendMessage ?? errorCodeService.getMessage(errorCode)
      };

      return throwError(() => enrichedError);
    })
  );
};

/**
 * Maneja errores basados en ErrorCode del backend
 */
function handleErrorCode(
  errorCode: ErrorCode,
  errorCodeService: ErrorCodeService,
  toastService: ToastService,
  router: Router,
  storageService: LocalStorageService,
  authService: AuthService
): void {
  // Si requiere re-autenticación, limpiar sesión y redirigir
  if (errorCodeService.requiresReauth(errorCode)) {
    storageService.clearSession();

    const currentUrl = router.url;
    if (!isPublicUrl(currentUrl)) {
      router.navigate(['/login'], {
        queryParams: { returnUrl: currentUrl }
      });
    }
    return;
  }

  // Mostrar toast según severidad (excepto para errores de validación que se manejan en forms)
  if (!errorCodeService.isValidationError(errorCode)) {
    const errorInfo = errorCodeService.getErrorInfo(errorCode);

    switch (errorInfo.severity) {
      case 'error':
        toastService.error(errorInfo.message);
        break;
      case 'warning':
        toastService.warning(errorInfo.message);
        break;
      case 'info':
        toastService.info(errorInfo.message);
        break;
    }
  }

  // Redirigir al dashboard del rol (CA-17)
  if (errorCode === ErrorCode.Forbidden || errorCode === ErrorCode.InsufficientPermissions) {
    const dashboard = RoleRoutes[authService.getUserRole() ?? ''] ?? '/login';
    router.navigate([dashboard]);
  }
}

/**
 * Fallback: maneja errores basados en HTTP status cuando no hay ErrorCode
 */
function handleHttpStatus(
  status: number,
  router: Router,
  toastService: ToastService,
  storageService: LocalStorageService,
  authService: AuthService
): void {
  switch (status) {
    case 401:
      storageService.clearSession();
      const currentUrl = router.url;
      if (!isPublicUrl(currentUrl)) {
        router.navigate(['/login'], {
          queryParams: { returnUrl: currentUrl }
        });
      }
      break;

    case 403: {
      toastService.error('No tenés permiso para acceder a este recurso');
      const dashboard = RoleRoutes[authService.getUserRole() ?? ''] ?? '/login';
      router.navigate([dashboard]);
      break;
    }

    case 500:
      toastService.error('Error interno del servidor');
      break;

    case 0:
      toastService.error('No se pudo conectar con el servidor');
      break;
  }
}

/**
 * Verifica si la URL actual es pública
 */
function isPublicUrl(url: string): boolean {
  return PUBLIC_URLS.some(publicUrl => url.includes(publicUrl));
}
