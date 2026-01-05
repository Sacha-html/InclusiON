import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';
import { guestGuard } from './guards/guest.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full',
  },

  // Visual login como pantalla principal de acceso
  {
    path: 'login',
    loadChildren: () =>
      import('./views/pages/visual-login/routes').then((m) => m.routes),
    canActivate: [guestGuard],
    data: {
      title: 'Iniciar Sesión',
      description: 'Acceso accesible a la plataforma',
    },
  },

  // Login tradicional para administradores/profesionales
  {
    path: 'admin-login',
    loadComponent: () =>
      import('./views/pages/login/login.component').then(
        (m) => m.LoginComponent
      ),
    canActivate: [guestGuard],
    data: {
      title: 'Acceso Administrativo',
      description: 'Acceso para administradores y profesionales',
    },
  },

  {
    path: '',
    loadComponent: () =>
      import('./layout').then((m) => m.DefaultLayoutComponent),
    canActivate: [authGuard],
    data: {
      title: 'Home',
    },
    children: [
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./views/dashboard/dashboard.component').then(
            (m) => m.DashboardComponent
          ),
        data: {
          title: 'Dashboard',
          description: 'Panel principal',
        },
      },
    ],
  },

  // ============================================
  // RUTAS DE ERROR
  // ============================================
  {
    path: '404',
    loadComponent: () =>
      import('./views/pages/page404/page404.component').then(
        (m) => m.Page404Component
      ),
    data: {
      title: 'Página No Encontrada',
    },
  },
  {
    path: '500',
    loadComponent: () =>
      import('./views/pages/page500/page500.component').then(
        (m) => m.Page500Component
      ),
    data: {
      title: 'Error del Servidor',
    },
  },

  {
    path: '**',
    redirectTo: '404',
  },
];
