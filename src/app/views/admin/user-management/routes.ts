import { Routes } from '@angular/router';

export const userManagementRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./list/list.component').then((m) => m.UserManagementListComponent),
    data: { title: 'Usuarios' },
  },
  {
    path: ':id',
    loadComponent: () =>
      import('./detail/detail.component').then((m) => m.UserManagementDetailComponent),
    data: { title: 'Detalle de Usuario' },
  },
];
