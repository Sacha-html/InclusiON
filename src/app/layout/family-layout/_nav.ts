import { INavData } from '@coreui/angular';

export const familyNavItems: INavData[] = [
  {
    name: 'Inicio',
    url: '/family',
    iconComponent: { name: 'cil-home' }
  },
  {
    title: true,
    name: 'Seguimiento'
  },
  {
    name: 'Actividades',
    url: '/family/activities',
    iconComponent: { name: 'cil-task' }
  },
  {
    name: 'Calendario',
    url: '/family/calendar',
    iconComponent: { name: 'cil-calendar' }
  },
  {
    name: 'Profesionales',
    url: '/family/professionals',
    iconComponent: { name: 'cil-medical-cross' }
  },
  {
    name: 'Progreso',
    url: '/family/progress',
    iconComponent: { name: 'cil-chart-line' }
  },
  {
    title: true,
    name: 'Comunicacion'
  },
  {
    name: 'Mensajes',
    url: '/family/messages',
    iconComponent: { name: 'cil-envelope-closed' }
  }
];
