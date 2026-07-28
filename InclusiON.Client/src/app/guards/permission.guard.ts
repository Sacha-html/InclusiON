import { inject } from '@angular/core';
import { CanActivateFn, ActivatedRouteSnapshot, Router } from '@angular/router';
import { AuthService, ToastService } from '@services';
import { AppRoutes } from '@shared/constants/app-routes';

export const permissionGuard: CanActivateFn = (route: ActivatedRouteSnapshot) => {
  const authService = inject(AuthService);
  const toastService = inject(ToastService);
  const router = inject(Router);

  const requiredPermission = route.data['permission'] as string | undefined;

  if (!requiredPermission) {
    return true;
  }

  if (authService.hasPermission(requiredPermission)) {
    return true;
  }

  toastService.warning('No tienes permisos para acceder a esta sección.');
  const redirectTo = route.data['redirectTo'] as string | undefined;
  router.navigate([redirectTo ?? AppRoutes.Admin.Dashboard]);
  return false;
};
