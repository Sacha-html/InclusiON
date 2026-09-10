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
    name: 'Comunidad',
  },
  {
    name: 'Alumnos',
    url: AppRoutes.Admin.Persons,
    iconComponent: { name: 'cil-people' },
  },
  {
    name: 'Profesionales',
    url: AppRoutes.Admin.Professionals,
    iconComponent: { name: 'cil-user' },
  },
  {
    name: 'Familiares',
    url: AppRoutes.Admin.Family,
    iconComponent: { name: 'cil-home' },
  },
  {
    name: 'Usuarios',
    url: AppRoutes.Admin.Users,
    iconComponent: { name: 'cil-group' },
  },
  {
    title: true,
    name: 'Seguimiento y Comunicación',
  },
  {
    name: 'Reportes',
    url: AppRoutes.Admin.Reports,
    iconComponent: { name: 'cil-description' },
  },
  {
    name: 'Mensajes',
    url: AppRoutes.Admin.Messages,
    iconComponent: { name: 'cil-envelope-closed' },
  },
  {
    name: 'Invitaciones',
    url: AppRoutes.Admin.Invitations,
    iconComponent: { name: 'cil-link' },
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
      { name: 'Áreas de habilidad', url: AppRoutes.Admin.Catalogs.SkillAreas },
      { name: 'Categorías de actividad', url: AppRoutes.Admin.Catalogs.ActivityCategories },
      { name: 'Métodos de login', url: AppRoutes.Admin.Catalogs.LoginMethods },
      { name: 'Niveles de autonomía', url: AppRoutes.Admin.Catalogs.AutonomyLevels },
      { name: 'Tipos de discapacidad', url: AppRoutes.Admin.Catalogs.DisabilityTypes },
      { name: 'Especialidades', url: AppRoutes.Admin.Catalogs.Specialties },
      { name: 'Tipos de plantilla', url: AppRoutes.Admin.Catalogs.TemplateTypes },
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
    name: 'Roles y permisos',
    url: AppRoutes.Admin.Roles,
    iconComponent: { name: 'cil-lock-locked' },
  },
];
