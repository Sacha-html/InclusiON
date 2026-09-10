import { INavData } from '@coreui/angular';
import { AppRoutes } from '@shared/constants/app-routes';

export const familyNavItems: INavData[] = [
  {
    name: 'Inicio',
    url: AppRoutes.Family.Root,
    iconComponent: { name: 'cil-home' }
  },
  {
    title: true,
    name: 'Seguimiento'
  },
  {
    name: 'Calendario',
    url: AppRoutes.Family.Calendar,
    iconComponent: { name: 'cil-calendar' }
  },
  {
    name: 'Actividades',
    url: AppRoutes.Family.Activities,
    iconComponent: { name: 'cil-task' }
  },
  {
    name: 'Reportes',
    url: AppRoutes.Family.Reports,
    iconComponent: { name: 'cil-description' }
  },
  {
    title: true,
    name: 'Comunicación'
  },
  {
    name: 'Mensajes',
    url: AppRoutes.Family.Messages,
    iconComponent: { name: 'cil-envelope-closed' }
  }
];
