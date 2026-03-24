import { Routes } from '@angular/router';

export const professionalRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./list/list.component').then((m) => m.ListComponent),
  },
  {
    path: 'new',
    loadComponent: () =>
      import('./new/new.component').then((m) => m.NewComponent),
  },
  {
    path: ':id',
    loadComponent: () =>
      import('./detail/detail.component').then((m) => m.DetailComponent),
  },
  {
    path: ':id/edit',
    loadComponent: () =>
      import('./edit/edit.component').then((m) => m.EditComponent),
  },
];
