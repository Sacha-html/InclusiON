import { Routes } from '@angular/router';

export const institutionRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./list/list.component').then((m) => m.ListComponent),
  },
  {
    path: 'new',
    loadComponent: () =>
      import('./new/new.component').then((m) => m.NewComponent),
    data: { title: 'Crear Institución' },
  },
  {
    path: ':id/edit',
    loadComponent: () =>
      import('./edit/edit.component').then((m) => m.EditComponent),
  },
];
