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
      import('./progress/family-progress.component').then(
        (m) => m.FamilyProgressComponent
      ),
    data: { title: 'Progreso' }
  },
  {
    path: 'activities',
    loadComponent: () =>
      import('./activities/family-activities.component').then(
        (m) => m.FamilyActivitiesComponent
      ),
    data: { title: 'Actividades' }
  },
  {
    path: 'calendar',
    loadComponent: () =>
      import('../coming-soon/coming-soon.component').then(
        (m) => m.ComingSoonComponent
      ),
    data: { title: 'Calendario' }
  },
  {
    path: 'messages',
    loadComponent: () =>
      import('../messages/messages.component').then(
        (m) => m.MessagesComponent
      ),
    data: { title: 'Mensajes' }
  },
  {
    path: 'professionals',
    loadComponent: () =>
      import('../coming-soon/coming-soon.component').then(
        (m) => m.ComingSoonComponent
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
