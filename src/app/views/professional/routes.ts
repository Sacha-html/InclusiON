import { Routes } from '@angular/router';

export const professionalRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./dashboard/detail/detail.component').then(
        (m) => m.DetailComponent
      ),
    data: { title: 'Dashboard Profesional' }
  },
  {
    path: 'persons',
    loadComponent: () =>
      import('./classroom/list/list.component').then(
        (m) => m.ListComponent
      ),
    data: { title: 'Mi Aula' }
  },
  {
    path: 'persons/:id',
    loadComponent: () =>
      import('./person-detail/person-detail.component').then(
        (m) => m.PersonDetailComponent
      ),
    data: { title: 'Detalle de Persona' }
  },
  {
    path: 'activities',
    loadComponent: () =>
      import('./activities/list/list.component').then(
        (m) => m.ListComponent
      ),
    data: { title: 'Actividades' }
  },
  {
    path: 'activities/new',
    loadComponent: () =>
      import('./activities/new/new.component').then(
        (m) => m.NewComponent
      ),
    data: { title: 'Nueva Actividad' }
  },
  {
    path: 'activities/:id/edit',
    loadComponent: () =>
      import('./activities/edit/edit.component').then(
        (m) => m.EditComponent
      ),
    data: { title: 'Editar Actividad' }
  },
  {
    path: 'goals',
    loadComponent: () =>
      import('./dashboard/detail/detail.component').then(
        (m) => m.DetailComponent
      ),
    data: { title: 'Objetivos' }
  },
  {
    path: 'evaluations',
    loadComponent: () =>
      import('./dashboard/detail/detail.component').then(
        (m) => m.DetailComponent
      ),
    data: { title: 'Evaluaciones' }
  },
  {
    path: 'reports',
    loadComponent: () =>
      import('./reports/list/list.component').then(
        (m) => m.ListComponent
      ),
    data: { title: 'Reportes' }
  },
  {
    path: 'reports/new',
    loadComponent: () =>
      import('./reports/new/new.component').then(
        (m) => m.NewComponent
      ),
    data: { title: 'Crear Reporte' }
  },
  {
    path: 'reports/:id/edit',
    loadComponent: () =>
      import('./reports/edit/edit.component').then(
        (m) => m.EditComponent
      ),
    data: { title: 'Editar Reporte' }
  },
  {
    path: 'reports/:id',
    loadComponent: () =>
      import('./reports/detail/detail.component').then(
        (m) => m.DetailComponent
      ),
    data: { title: 'Detalle de Reporte' }
  },
  {
    path: 'messages',
    loadComponent: () =>
      import('./dashboard/detail/detail.component').then(
        (m) => m.DetailComponent
      ),
    data: { title: 'Mensajes' }
  },
  {
    path: 'calendar',
    loadComponent: () =>
      import('./dashboard/detail/detail.component').then(
        (m) => m.DetailComponent
      ),
    data: { title: 'Calendario' }
  },
  {
    path: 'invitations',
    loadComponent: () =>
      import('./invitations/list/list.component').then(
        (m) => m.ListComponent
      ),
    data: { title: 'Invitaciones' }
  }
];
