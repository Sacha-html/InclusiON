import { Routes } from '@angular/router';

export const professionalRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./dashboard/pro-dashboard.component').then(
        (m) => m.ProDashboardComponent
      ),
    data: { title: 'Dashboard Profesional' }
  },
  {
    path: 'persons',
    loadComponent: () =>
      import('./dashboard/pro-dashboard.component').then(
        (m) => m.ProDashboardComponent
      ),
    data: { title: 'Personas a Cargo' }
  },
  {
    path: 'activities',
    loadComponent: () =>
      import('./dashboard/pro-dashboard.component').then(
        (m) => m.ProDashboardComponent
      ),
    data: { title: 'Actividades' }
  },
  {
    path: 'goals',
    loadComponent: () =>
      import('./dashboard/pro-dashboard.component').then(
        (m) => m.ProDashboardComponent
      ),
    data: { title: 'Objetivos' }
  },
  {
    path: 'evaluations',
    loadComponent: () =>
      import('./dashboard/pro-dashboard.component').then(
        (m) => m.ProDashboardComponent
      ),
    data: { title: 'Evaluaciones' }
  },
  {
    path: 'reports',
    loadComponent: () =>
      import('./dashboard/pro-dashboard.component').then(
        (m) => m.ProDashboardComponent
      ),
    data: { title: 'Reportes' }
  },
  {
    path: 'messages',
    loadComponent: () =>
      import('./dashboard/pro-dashboard.component').then(
        (m) => m.ProDashboardComponent
      ),
    data: { title: 'Mensajes' }
  },
  {
    path: 'calendar',
    loadComponent: () =>
      import('./dashboard/pro-dashboard.component').then(
        (m) => m.ProDashboardComponent
      ),
    data: { title: 'Calendario' }
  }
];
