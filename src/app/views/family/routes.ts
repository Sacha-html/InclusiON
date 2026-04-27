import { Routes } from '@angular/router';

export const familyRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./dashboard/family-dashboard.component').then(
        (m) => m.FamilyDashboardComponent
      ),
    data: { title: 'Inicio' }
  },
  {
    path: 'progress',
    loadComponent: () =>
      import('./dashboard/family-dashboard.component').then(
        (m) => m.FamilyDashboardComponent
      ),
    data: { title: 'Progreso' }
  },
  {
    path: 'activities',
    loadComponent: () =>
      import('./dashboard/family-dashboard.component').then(
        (m) => m.FamilyDashboardComponent
      ),
    data: { title: 'Actividades' }
  },
  {
    path: 'calendar',
    loadComponent: () =>
      import('./dashboard/family-dashboard.component').then(
        (m) => m.FamilyDashboardComponent
      ),
    data: { title: 'Calendario' }
  },
  {
    path: 'messages',
    loadComponent: () =>
      import('./dashboard/family-dashboard.component').then(
        (m) => m.FamilyDashboardComponent
      ),
    data: { title: 'Mensajes' }
  },
  {
    path: 'professionals',
    loadComponent: () =>
      import('./dashboard/family-dashboard.component').then(
        (m) => m.FamilyDashboardComponent
      ),
    data: { title: 'Profesionales' }
  },
  {
    path: 'reports',
    loadChildren: () =>
      import('./reports/routes').then((m) => m.familyReportRoutes),
    data: { title: 'Reportes' }
  }
];
