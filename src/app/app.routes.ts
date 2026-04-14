import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';
import { guestGuard } from './guards/guest.guard';
import { roleGuard } from './guards/role.guard';
import { globalAdminGuard } from './guards/global-admin.guard';
import { permissionGuard } from './guards/permission.guard';
import { UserRoles } from './shared/constants/roles';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    canActivate: [guestGuard],
    // Si no esta logueado: guestGuard deja pasar -> carga login
    // Si esta logueado: guestGuard redirige al dashboard del rol
    loadComponent: () =>
      import('./views/pages/visual-login/role-selection.component').then(
        (m) => m.RoleSelectionComponent,
      ),
  },

  // Visual login
  {
    path: 'login',
    loadChildren: () =>
      import('./views/pages/visual-login/routes').then((m) => m.routes),
    canActivate: [guestGuard],
  },

  // Login tradicional para administradores/profesionales
  {
    path: 'admin-login',
    loadComponent: () =>
      import('./views/pages/login/login.component').then(
        (m) => m.LoginComponent,
      ),
    canActivate: [guestGuard],
  },

  // Registro por invitacion familiar (publico)
  {
    path: 'invite/:code',
    loadComponent: () =>
      import('./views/pages/register-by-invitation/register-by-invitation.component').then(
        (m) => m.RegisterByInvitationComponent,
      ),
  },

  // Registro público de profesionales
  {
    path: 'register-professional',
    loadComponent: () =>
      import('./views/pages/register-professional/register-professional.component').then(
        (m) => m.RegisterProfessionalComponent,
      ),
  },

  // Cambio de contraseña obligatorio
  {
    path: 'change-password',
    loadComponent: () =>
      import('./views/pages/change-password/change-password.component').then(
        (m) => m.ChangePasswordComponent,
      ),
    canActivate: [authGuard],
  },

  // Dashboard AAC - Persona con Discapacidad
  {
    path: 'app',
    loadComponent: () =>
      import('./layout/aac-layout/aac-layout.component').then(
        (m) => m.AacLayoutComponent,
      ),
    canActivate: [authGuard, roleGuard],
    data: { roles: [UserRoles.PersonWithDisability] },
    loadChildren: () => import('./views/aac/routes').then((m) => m.aacRoutes),
  },

  // Dashboard Profesional
  {
    path: 'pro',
    loadComponent: () =>
      import('./layout/professional-layout/professional-layout.component').then(
        (m) => m.ProfessionalLayoutComponent,
      ),
    canActivate: [authGuard, roleGuard],
    data: { roles: [UserRoles.Professional, UserRoles.Admin] },
    loadChildren: () =>
      import('./views/professional/routes').then((m) => m.professionalRoutes),
  },

  // Dashboard Familia
  {
    path: 'family',
    loadComponent: () =>
      import('./layout/family-layout/family-layout.component').then(
        (m) => m.FamilyLayoutComponent,
      ),
    canActivate: [authGuard, roleGuard],
    data: { roles: [UserRoles.FamilyRepresentative, UserRoles.Admin] },
    loadChildren: () =>
      import('./views/family/routes').then((m) => m.familyRoutes),
  },

  // Dashboard Admin (layout existente)
  {
    path: 'admin',
    loadComponent: () =>
      import('./layout').then((m) => m.DefaultLayoutComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: [UserRoles.Admin] },
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full',
      },
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./views/admin/dashboard/dashboard.component').then(
            (m) => m.DashboardComponent,
          ),
      },
      {
        path: 'professionals',
        data: { title: 'Profesionales' },
        loadChildren: () =>
          import('./views/admin/professionals/routes').then(
            (m) => m.professionalRoutes,
          ),
      },
      {
        path: 'persons',
        data: { title: 'Personas' },
        loadChildren: () =>
          import('./views/admin/persons/routes').then(
            (m) => m.personRoutes,
          ),
      },
      {
        path: 'family',
        data: { title: 'Familiares' },
        loadChildren: () =>
          import('./views/admin/family/routes').then(
            (m) => m.familyRoutes,
          ),
      },
      {
        path: 'institutions',
        data: { title: 'Instituciones' },
        canActivate: [globalAdminGuard],
        loadChildren: () =>
          import('./views/admin/institutions/routes').then(
            (m) => m.institutionRoutes,
          ),
      },
      {
        path: 'catalogs/:type',
        data: { title: 'Catalogos' },
        loadComponent: () =>
          import('./views/admin/catalogs/catalogs.component').then(
            (m) => m.CatalogsComponent,
          ),
      },
      {
        path: 'reports',
        data: { title: 'Reportes' },
        loadChildren: () =>
          import('./views/admin/reports/routes').then(
            (m) => m.adminReportRoutes,
          ),
      },
      {
        path: 'invitations',
        data: { title: 'Invitaciones', permission: 'invitations:read' },
        canActivate: [permissionGuard],
        loadComponent: () =>
          import('./views/admin/invitations/invitations.component').then(
            (m) => m.InvitationsComponent,
          ),
      },
      {
        path: 'my-institutions',
        data: { title: 'Mis Instituciones' },
        loadComponent: () =>
          import('./views/admin/admin-institutions/admin-institutions.component').then(
            (m) => m.AdminInstitutionsComponent,
          ),
      },
      {
        path: 'users',
        data: { title: 'Gestión de Usuarios' },
        loadChildren: () =>
          import('./views/admin/user-management/routes').then(
            (m) => m.userManagementRoutes,
          ),
      },
      {
        path: 'admins',
        data: { title: 'Administradores' },
        canActivate: [globalAdminGuard],
        children: [
          {
            path: '',
            loadComponent: () =>
              import('./views/admin/admin-users/admin-users.component').then(
                (m) => m.AdminUsersComponent,
              ),
          },
          {
            path: 'new',
            loadComponent: () =>
              import('./views/admin/admin-users/new/new.component').then(
                (m) => m.NewComponent,
              ),
          },
        ],
      },
      {
        path: 'roles',
        data: { title: 'Roles y Permisos' },
        canActivate: [globalAdminGuard],
        loadComponent: () =>
          import('./views/admin/roles/roles.component').then(
            (m) => m.RolesComponent,
          ),
      },
    ],
  },

  // Ruta legacy
  {
    path: 'dashboard',
    redirectTo: 'admin/dashboard',
    pathMatch: 'full',
  },

  // Errores
  {
    path: '404',
    loadComponent: () =>
      import('./views/pages/page404/page404.component').then(
        (m) => m.Page404Component,
      ),
  },
  {
    path: '500',
    loadComponent: () =>
      import('./views/pages/page500/page500.component').then(
        (m) => m.Page500Component,
      ),
  },

  {
    path: '**',
    redirectTo: '404',
  },
];
