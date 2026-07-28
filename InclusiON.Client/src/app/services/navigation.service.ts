import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';
import { RoleRoutes } from '@shared/constants/roles';

@Injectable({
  providedIn: 'root'
})
export class NavigationService {
  private router = inject(Router);
  private authService = inject(AuthService);

  navigateToRoleDashboard(): void {
    const user = this.authService.getCurrentUser();

    if (!user || !user.role) {
      this.router.navigate(['/login']);
      return;
    }

    const targetRoute = RoleRoutes[user.role] || '/login';
    this.router.navigate([targetRoute]);
  }

  getHomeRoute(): string {
    const user = this.authService.getCurrentUser();
    if (!user || !user.role) return '/login';

    return RoleRoutes[user.role] || '/login';
  }

  navigateToHome(): void {
    const homeRoute = this.getHomeRoute();
    this.router.navigate([homeRoute]);
  }
}
