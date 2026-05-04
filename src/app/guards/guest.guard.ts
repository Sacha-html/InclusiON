import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services';
import { RoleRoutes } from '../shared/constants/roles';
import { AppRoutes } from '../shared/constants/app-routes';

export const guestGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    return true;
  }

  const user = authService.getCurrentUser();
  if (user && user.role) {
    const targetRoute = RoleRoutes[user.role] || AppRoutes.Aac.Root;
    router.navigate([targetRoute]);
  } else {
    router.navigate([AppRoutes.Aac.Root]);
  }

  return false;
};
