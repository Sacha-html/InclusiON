import { INavData } from '@coreui/angular';
import { AppRoutes } from '@shared/constants/app-routes';

export const professionalNavItems: INavData[] = [
  {
    name: 'Dashboard',
    url: AppRoutes.Pro.Root,
    iconComponent: { name: 'cil-speedometer' },
  },
  {
    title: true,
    name: 'Gestión'
  },
  {
    name: 'Actividades',
    url: AppRoutes.Pro.Activities,
    iconComponent: { name: 'cil-task' }
  },
  {
    name: 'Mi aula',
    url: AppRoutes.Pro.Persons,
    iconComponent: { name: 'cil-people' }
  },
  {
    name: 'Objetivos',
    url: AppRoutes.Pro.Goals,
    iconComponent: { name: 'cil-star' }
  },
  {
    title: true,
    name: 'Evaluación'
  },
  {
    name: 'Evaluaciones',
    url: AppRoutes.Pro.Evaluations,
    iconComponent: { name: 'cil-clipboard' }
  },
  {
    name: 'Reportes',
    url: AppRoutes.Pro.Reports,
    iconComponent: { name: 'cil-chart' }
  },
  {
    title: true,
    name: 'Comunicación'
  },
  {
    name: 'Calendario',
    url: AppRoutes.Pro.Calendar,
    iconComponent: { name: 'cil-calendar' }
  },
  {
    name: 'Invitaciones',
    url: AppRoutes.Pro.Invitations,
    iconComponent: { name: 'cil-link' }
  },
  {
    name: 'Mensajes',
    url: AppRoutes.Pro.Messages,
    iconComponent: { name: 'cil-envelope-closed' }
  }
];
