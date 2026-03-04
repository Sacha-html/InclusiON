import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./role-selection.component').then((m) => m.RoleSelectionComponent),
    data: {
      title: 'Seleccionar Rol',
    },
  },
  {
    path: 'identify',
    loadComponent: () =>
      import('./identify-user.component').then((m) => m.IdentifyUserComponent),
    data: {
      title: 'Identificarse',
    },
  },
  {
    path: 'standard',
    loadComponent: () =>
      import('./visual-standard-login.component').then((m) => m.VisualStandardLoginComponent),
    data: {
      title: 'Ingresar Contrasena',
    },
  },
  {
    path: 'pin',
    loadComponent: () =>
      import('./pin-login.component').then((m) => m.PinLoginComponent),
    data: {
      title: 'Ingresar PIN',
    },
  },
  {
    path: 'family',
    loadComponent: () =>
      import('./family-login.component').then((m) => m.FamilyLoginComponent),
    data: {
      title: 'Login Familiar',
    },
  },
  {
    path: 'assisted',
    loadComponent: () =>
      import('./assisted-login.component').then((m) => m.AssistedLoginComponent),
    data: {
      title: 'Login Asistido',
    },
  },
  {
    path: 'settings/login-method',
    loadComponent: () =>
      import('./login-method-selector.component').then((m) => m.LoginMethodSelectorComponent),
    data: {
      title: 'Configurar Metodo de Login',
    },
  },
];
