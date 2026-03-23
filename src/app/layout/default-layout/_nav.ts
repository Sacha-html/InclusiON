import { INavData } from '@coreui/angular';

export const navItems: INavData[] = [
  {
    name: 'Dashboard',
    url: '/admin/dashboard',
    iconComponent: { name: 'cil-speedometer' },
  },
  {
    title: true,
    name: 'Gestion',
  },
  {
    name: 'Profesionales',
    url: '/admin/professionals',
    iconComponent: { name: 'cil-user' },
  },
  {
    name: 'Personas',
    url: '/admin/persons',
    iconComponent: { name: 'cil-people' },
  },
  {
    name: 'Familiares',
    url: '/admin/family',
    iconComponent: { name: 'cil-home' },
  },
  {
    name: 'Instituciones',
    url: '/admin/institutions',
    iconComponent: { name: 'cil-book' },
  },
  {
    name: 'Invitaciones',
    url: '/admin/invitations',
    iconComponent: { name: 'cil-link' },
  },
  {
    title: true,
    name: 'Configuracion',
  },
  {
    name: 'Catalogos',
    url: '/admin/catalogs',
    iconComponent: { name: 'cil-notes' },
    children: [
      { name: 'Tipos de Discapacidad', url: '/admin/catalogs/disability-types' },
      { name: 'Niveles de Autonomia', url: '/admin/catalogs/autonomy-levels' },
      { name: 'Categorias de Actividad', url: '/admin/catalogs/activity-categories' },
      { name: 'Areas de Habilidad', url: '/admin/catalogs/skill-areas' },
      { name: 'Tipos de Template', url: '/admin/catalogs/template-types' },
      { name: 'Metodos de Login', url: '/admin/catalogs/login-methods' },
    ],
  },
  {
    name: 'Mis Instituciones',
    url: '/admin/my-institutions',
    iconComponent: { name: 'cil-book' },
  },
  {
    title: true,
    name: 'Sistema',
  },
  {
    name: 'Administradores',
    url: '/admin/admins',
    iconComponent: { name: 'cil-settings' },
  },
  {
    name: 'Roles y Permisos',
    url: '/admin/roles',
    iconComponent: { name: 'cil-lock-locked' },
  },
];
