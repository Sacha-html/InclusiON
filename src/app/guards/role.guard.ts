import { inject } from '@angular/core';
import { CanActivateFn, Router, ActivatedRouteSnapshot } from '@angular/router';
import { AuthService } from '../services';
import { RoleRoutes } from '../shared/constants/roles';

export const roleGuard: CanActivateFn = (route: ActivatedRouteSnapshot) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const allowedRoles = route.data['roles'] as string[] | undefined;

  if (!allowedRoles || allowedRoles.length === 0) {
    return true;
  }

  const user = authService.getCurrentUser();

  if (!user || !user.role) {
    router.navigate(['/login']);
    return false;
  }

  if (allowedRoles.includes(user.role)) {
    return true;
  }

  const targetRoute = RoleRoutes[user.role] || '/login';
  router.navigate([targetRoute]);
  return false;
};
