import { inject } from '@angular/core';
import { CanActivateFn, Router, ActivatedRouteSnapshot } from '@angular/router';
import { AuthService, ToastService } from '@services';
import { RoleRoutes } from '@shared/constants/roles';
import { AppRoutes } from '@shared/constants/app-routes';

export const roleGuard: CanActivateFn = (route: ActivatedRouteSnapshot) => {
  const authService = inject(AuthService);
  const toastService = inject(ToastService);
  const router = inject(Router);

  let currentRoute: ActivatedRouteSnapshot | null = route;
  let allowedRoles: string[] | undefined;

  while (currentRoute) {
    if (currentRoute.data && currentRoute.data['roles'] && (currentRoute.data['roles'] as string[]).length > 0) {
      allowedRoles = currentRoute.data['roles'] as string[];
      break;
    }
    currentRoute = currentRoute.parent;
  }

  if (!allowedRoles || allowedRoles.length === 0) {
    return true;
  }

  const user = authService.getCurrentUser();

  if (!user || !user.role) {
    router.navigate([AppRoutes.Login]);
    return false;
  }

  const userRoleLower = user.role.toLowerCase();
  const hasAccess = allowedRoles.some((r) => r.toLowerCase() === userRoleLower);

  if (hasAccess) {
    return true;
  }

  toastService.warning('No tienes permisos para acceder a esta sección.');
  const targetRoute = RoleRoutes[user.role] || '/';
  router.navigate([targetRoute]);
  return false;
};
