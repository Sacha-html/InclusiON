import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);

  const token = localStorage.getItem('access_token');

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
      if (error.status === 401) {
        localStorage.removeItem('access_token');
        localStorage.removeItem('refresh_token');
        localStorage.removeItem('current_user');

        const publicUrls = ['/login', '/register', '/forgot-password'];
        const currentUrl = router.url;

        if (!publicUrls.some((url) => currentUrl.includes(url))) {
          router.navigate(['/login'], {
            queryParams: { returnUrl: currentUrl },
          });
        }
      }

      if (error.status === 403) {
        console.error('🚫 Error 403: Sin permisos para acceder a este recurso');
        router.navigate(['/403']);
      }

      if (error.status === 500) {
        console.error('⚠️ Error 500: Error en el servidor');
        // Podrías mostrar un toast o notificación aquí
      }

      return throwError(() => error);
    })
  );
};
