import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services';
import { RoleRoutes } from '../shared/constants/roles';

export const guestGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    return true;
  }

  const user = authService.getCurrentUser();
  if (user && user.role) {
    const targetRoute = RoleRoutes[user.role] || '/app';
    router.navigate([targetRoute]);
  } else {
    router.navigate(['/app']);
  }

  return false;
};
