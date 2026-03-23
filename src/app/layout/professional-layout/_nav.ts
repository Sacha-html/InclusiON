import { INavData } from '@coreui/angular';

export const professionalNavItems: INavData[] = [
  {
    name: 'Dashboard',
    url: '/pro',
    iconComponent: { name: 'cil-speedometer' },
    badge: {
      color: 'info',
      text: 'PRO'
    }
  },
  {
    title: true,
    name: 'Gestion'
  },
  {
    name: 'Mi Aula',
    url: '/pro/persons',
    iconComponent: { name: 'cil-people' }
  },
  {
    name: 'Actividades',
    url: '/pro/activities',
    iconComponent: { name: 'cil-task' }
  },
  {
    name: 'Objetivos',
    url: '/pro/goals',
    iconComponent: { name: 'cil-star' }
  },
  {
    title: true,
    name: 'Evaluacion'
  },
  {
    name: 'Evaluaciones',
    url: '/pro/evaluations',
    iconComponent: { name: 'cil-clipboard' }
  },
  {
    name: 'Reportes',
    url: '/pro/reports',
    iconComponent: { name: 'cil-chart' }
  },
  {
    title: true,
    name: 'Comunicacion'
  },
  {
    name: 'Mensajes',
    url: '/pro/messages',
    iconComponent: { name: 'cil-envelope-closed' }
  },
  {
    name: 'Calendario',
    url: '/pro/calendar',
    iconComponent: { name: 'cil-calendar' }
  },
  {
    name: 'Invitaciones',
    url: '/pro/invitations',
    iconComponent: { name: 'cil-link' }
  }
];
