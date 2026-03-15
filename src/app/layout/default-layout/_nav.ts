import { INavData } from '@coreui/angular';

export const navItems: INavData[] = [
  {
    name: 'Dashboard',
    url: '/admin/dashboard',
    iconComponent: { name: 'cil-speedometer' },
  },
  {
    name: 'Profesionales',
    url: '/admin/professionals',
    iconComponent: { name: 'cil-user' },
  },
];
