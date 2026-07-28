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
    name: 'Actividades',
    url: AppRoutes.Family.Activities,
    iconComponent: { name: 'cil-task' }
  },
  {
    name: 'Calendario',
    url: AppRoutes.Family.Calendar,
    iconComponent: { name: 'cil-calendar' }
  },
  {
    name: 'Profesionales',
    url: AppRoutes.Family.Professionals,
    iconComponent: { name: 'cil-medical-cross' }
  },
  {
    name: 'Progreso',
    url: AppRoutes.Family.Progress,
    iconComponent: { name: 'cil-chart-line' }
  },
  {
    name: 'Reportes',
    url: AppRoutes.Family.Reports,
    iconComponent: { name: 'cil-description' }
  },
  {
    title: true,
    name: 'Comunicacion'
  },
  {
    name: 'Mensajes',
    url: AppRoutes.Family.Messages,
    iconComponent: { name: 'cil-envelope-closed' }
  }
];
