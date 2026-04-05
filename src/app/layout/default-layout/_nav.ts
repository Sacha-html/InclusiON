import { INavData } from '@coreui/angular';

export const navItems: INavData[] = [
  {
    name: 'Dashboard',
    url: '/admin/dashboard',
    iconComponent: { name: 'cil-speedometer' },
  },
  {
    title: true,
    name: 'Gestión',
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
    name: 'Personas',
    url: '/admin/persons',
    iconComponent: { name: 'cil-people' },
  },
  {
    name: 'Profesionales',
    url: '/admin/professionals',
    iconComponent: { name: 'cil-user' },
  },
  {
    name: 'Usuarios',
    url: '/admin/users',
    iconComponent: { name: 'cil-group' },
  },
  {
    title: true,
    name: 'Configuración',
  },
  {
    name: 'Catálogos',
    url: '/admin/catalogs',
    iconComponent: { name: 'cil-notes' },
    children: [
      { name: 'Áreas de Habilidad', url: '/admin/catalogs/skill-areas' },
      { name: 'Categorías de Actividad', url: '/admin/catalogs/activity-categories' },
      { name: 'Métodos de Login', url: '/admin/catalogs/login-methods' },
      { name: 'Niveles de Autonomía', url: '/admin/catalogs/autonomy-levels' },
      { name: 'Tipos de Discapacidad', url: '/admin/catalogs/disability-types' },
      { name: 'Tipos de Plantilla', url: '/admin/catalogs/template-types' },
    ],
  },
  {
    name: 'Mis Instituciones',
    url: '/admin/my-institutions',
    iconComponent: { name: 'cil-library' },
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
