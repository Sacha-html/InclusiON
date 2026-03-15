import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';
import { guestGuard } from './guards/guest.guard';
import { roleGuard } from './guards/role.guard';
import { UserRoles } from './shared/constants/roles';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full',
  },

  // Visual login
  {
    path: 'login',
    loadChildren: () =>
      import('./views/pages/visual-login/routes').then((m) => m.routes),
    canActivate: [guestGuard],
  },

  // Login tradicional para administradores/profesionales
  {
    path: 'admin-login',
    loadComponent: () =>
      import('./views/pages/login/login.component').then(
        (m) => m.LoginComponent,
      ),
    canActivate: [guestGuard],
  },

  // Dashboard AAC - Persona con Discapacidad
  {
    path: 'app',
    loadComponent: () =>
      import('./layout/aac-layout/aac-layout.component').then(
        (m) => m.AacLayoutComponent,
      ),
    canActivate: [authGuard, roleGuard],
    data: { roles: [UserRoles.PersonWithDisability] },
    loadChildren: () => import('./views/aac/routes').then((m) => m.aacRoutes),
  },

  // Dashboard Profesional
  {
    path: 'pro',
    loadComponent: () =>
      import('./layout/professional-layout/professional-layout.component').then(
        (m) => m.ProfessionalLayoutComponent,
      ),
    canActivate: [authGuard, roleGuard],
    data: { roles: [UserRoles.Professional, UserRoles.Admin] },
    loadChildren: () =>
      import('./views/professional/routes').then((m) => m.professionalRoutes),
  },

  // Dashboard Familia
  {
    path: 'family',
    loadComponent: () =>
      import('./layout/family-layout/family-layout.component').then(
        (m) => m.FamilyLayoutComponent,
      ),
    canActivate: [authGuard, roleGuard],
    data: { roles: [UserRoles.FamilyRepresentative, UserRoles.Admin] },
    loadChildren: () =>
      import('./views/family/routes').then((m) => m.familyRoutes),
  },

  // Dashboard Admin (layout existente)
  {
    path: 'admin',
    loadComponent: () =>
      import('./layout').then((m) => m.DefaultLayoutComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: [UserRoles.Admin] },
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full',
      },
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./views/admin/dashboard/dashboard.component').then(
            (m) => m.DashboardComponent,
          ),
      },
      {
        path: 'professionals',
        loadChildren: () =>
          import('./views/admin/professionals/routes').then(
            (m) => m.professionalRoutes,
          ),
      },
    ],
  },

  // Ruta legacy
  {
    path: 'dashboard',
    redirectTo: 'admin/dashboard',
    pathMatch: 'full',
  },

  // Errores
  {
    path: '404',
    loadComponent: () =>
      import('./views/pages/page404/page404.component').then(
        (m) => m.Page404Component,
      ),
  },
  {
    path: '500',
    loadComponent: () =>
      import('./views/pages/page500/page500.component').then(
        (m) => m.Page500Component,
      ),
  },

  {
    path: '**',
    redirectTo: '404',
  },
];
