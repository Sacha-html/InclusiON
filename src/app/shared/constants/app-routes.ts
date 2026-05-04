/**
 * Rutas de la aplicación.
 * Usar en router.navigate(), routerLink y data de rutas en lugar de strings literales.
 */
export const AppRoutes = {
  // ── Auth ────────────────────────────────────────────────────────────
  Login:          '/login',
  AdminLogin:     '/admin-login',
  VisualLogin:    '/visual-login',
  Register:       '/register',
  Invite:         '/invite',
  ChangePassword: '/change-password',

  // Métodos de login visual
  LoginFamily:   '/login/family',
  LoginStandard: '/login/standard',
  LoginPin:      '/login/pin',

  // ── Admin ────────────────────────────────────────────────────────────
  Admin: {
    Root:          '/admin',
    Dashboard:     '/admin/dashboard',
    Family:        '/admin/family',
    Institutions:  '/admin/institutions',
    Invitations:   '/admin/invitations',
    Persons:       '/admin/persons',
    Professionals: '/admin/professionals',
    Users:         '/admin/users',
    Reports:       '/admin/reports',
    MyInstitutions:'/admin/my-institutions',
    Admins:        '/admin/admins',
    Roles:         '/admin/roles',
    Catalogs: {
      Root:               '/admin/catalogs',
      SkillAreas:         '/admin/catalogs/skill-areas',
      ActivityCategories: '/admin/catalogs/activity-categories',
      LoginMethods:       '/admin/catalogs/login-methods',
      AutonomyLevels:     '/admin/catalogs/autonomy-levels',
      DisabilityTypes:    '/admin/catalogs/disability-types',
      TemplateTypes:      '/admin/catalogs/template-types',
    },
  },

  // ── Profesional ──────────────────────────────────────────────────────
  Pro: {
    Root:        '/pro',
    Activities:  '/pro/activities',
    ActivityNew: '/pro/activities/new',
    Persons:     '/pro/persons',
    Goals:       '/pro/goals',
    Evaluations: '/pro/evaluations',
    Reports:     '/pro/reports',
    Calendar:    '/pro/calendar',
    Invitations: '/pro/invitations',
    Messages:    '/pro/messages',
  },

  // ── Familia ──────────────────────────────────────────────────────────
  Family: {
    Root:          '/family',
    Activities:    '/family/activities',
    Calendar:      '/family/calendar',
    Professionals: '/family/professionals',
    Progress:      '/family/progress',
    Reports:       '/family/reports',
    Messages:      '/family/messages',
  },

  // ── AAC (Persona con Discapacidad) ───────────────────────────────────
  Aac: {
    Root:       '/app',
    Activities: '/app/activities',
    Calendar:   '/app/calendar',
    Talk:       '/app/talk',
  },
} as const;
