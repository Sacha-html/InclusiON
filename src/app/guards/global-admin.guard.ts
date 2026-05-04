import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService, ToastService } from '../services';
import { AppRoutes } from '../shared/constants/app-routes';

export const globalAdminGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const toastService = inject(ToastService);
  const router = inject(Router);

  if (authService.isGlobalAdmin()) {
    return true;
  }

  toastService.warning('No tienes permisos para acceder a esta sección.');
  router.navigate([AppRoutes.Admin.Dashboard]);
  return false;
};
