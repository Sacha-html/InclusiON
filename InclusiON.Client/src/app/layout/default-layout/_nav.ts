import { INavData } from '@coreui/angular';
import { AppRoutes } from '@shared/constants/app-routes';

export const navItems: INavData[] = [
  {
    name: 'Dashboard',
    url: AppRoutes.Admin.Dashboard,
    iconComponent: { name: 'cil-speedometer' },
  },
  {
    title: true,
    name: 'Gestión',
  },
  {
    name: 'Familiares',
    url: AppRoutes.Admin.Family,
    iconComponent: { name: 'cil-home' },
  },
  {
    name: 'Invitaciones',
    url: AppRoutes.Admin.Invitations,
    iconComponent: { name: 'cil-link' },
  },
  {
    name: 'Personas',
    url: AppRoutes.Admin.Persons,
    iconComponent: { name: 'cil-people' },
  },
  {
    name: 'Profesionales',
    url: AppRoutes.Admin.Professionals,
    iconComponent: { name: 'cil-user' },
  },
  {
    name: 'Usuarios',
    url: AppRoutes.Admin.Users,
    iconComponent: { name: 'cil-group' },
  },
  {
    name: 'Reportes',
    url: AppRoutes.Admin.Reports,
    iconComponent: { name: 'cil-description' },
  },
  {
    title: true,
    name: 'Configuración',
  },
  {
    name: 'Catálogos',
    url: AppRoutes.Admin.Catalogs.Root,
    iconComponent: { name: 'cil-notes' },
    children: [
      { name: 'Áreas de Habilidad', url: AppRoutes.Admin.Catalogs.SkillAreas },
      { name: 'Categorías de Actividad', url: AppRoutes.Admin.Catalogs.ActivityCategories },
      { name: 'Métodos de Login', url: AppRoutes.Admin.Catalogs.LoginMethods },
      { name: 'Niveles de Autonomía', url: AppRoutes.Admin.Catalogs.AutonomyLevels },
      { name: 'Tipos de Discapacidad', url: AppRoutes.Admin.Catalogs.DisabilityTypes },
      { name: 'Tipos de Plantilla', url: AppRoutes.Admin.Catalogs.TemplateTypes },
    ],
  },
  {
    title: true,
    name: 'Sistema',
  },
  {
    name: 'Administradores',
    url: AppRoutes.Admin.Admins,
    iconComponent: { name: 'cil-settings' },
  },
  {
    name: 'Roles y Permisos',
    url: AppRoutes.Admin.Roles,
    iconComponent: { name: 'cil-lock-locked' },
  },
];
