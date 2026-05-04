import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services';
import { AppRoutes } from '../shared/constants/app-routes';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return true;
  }

  router.navigate([AppRoutes.Login], {
    queryParams: { returnUrl: state.url },
  });

  return false;
};
