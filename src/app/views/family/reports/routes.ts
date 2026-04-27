import { Routes } from '@angular/router';

export const familyReportRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./list/list.component').then((m) => m.ListComponent),
    data: { title: 'Reportes' },
  },
  {
    path: ':id',
    loadComponent: () =>
      import('./detail/detail.component').then((m) => m.DetailComponent),
    data: { title: 'Detalle de Reporte' },
  },
];
