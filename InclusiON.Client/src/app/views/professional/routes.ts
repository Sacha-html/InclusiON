import { Routes } from '@angular/router';
import { permissionGuard } from '@guards/permission.guard';
import { Permissions } from '@shared/constants/permissions';
import { AppRoutes } from '@shared/constants/app-routes';

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
    canActivate: [permissionGuard],
    data: { title: 'Nueva Actividad', permission: Permissions.Activities.Create, redirectTo: AppRoutes.Pro.Activities }
  },
  {
    path: 'activities/:id',
    loadComponent: () =>
      import('./activities/detail/detail.component').then(
        (m) => m.DetailComponent
      ),
    data: { title: 'Detalle de Actividad' }
  },
  {
    path: 'activities/:id/edit',
    loadComponent: () =>
      import('./activities/edit/edit.component').then(
        (m) => m.EditComponent
      ),
    canActivate: [permissionGuard],
    data: { title: 'Editar Actividad', permission: Permissions.Activities.Update, redirectTo: AppRoutes.Pro.Activities }
  },
  {
    path: 'goals',
    loadComponent: () =>
      import('./goals/professional-goals.component').then(
        (m) => m.ProfessionalGoalsComponent
      ),
    data: { title: 'Objetivos' }
  },
  {
    path: 'evaluations',
    loadComponent: () =>
      import('./evaluations/evaluations.component').then(
        (m) => m.EvaluationsComponent
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
      import('../messages/messages.component').then(
        (m) => m.MessagesComponent
      ),
    data: { title: 'Mensajes' }
  },
  {
    path: 'calendar',
    loadComponent: () =>
      import('../calendar/calendar.component').then(
        (m) => m.CalendarComponent
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
