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
    path: 'pin',
    loadComponent: () =>
      import('./pin-login.component').then((m) => m.PinLoginComponent),
    data: {
      title: 'Ingresar PIN',
    },
  },
  {
    path: 'emoji',
    loadComponent: () =>
      import('./emoji-login.component').then((m) => m.EmojiLoginComponent),
    data: {
      title: 'Seleccionar Emojis',
    },
  },
  {
    path: 'color-shape',
    loadComponent: () =>
      import('./color-shape-login.component').then((m) => m.ColorShapeLoginComponent),
    data: {
      title: 'Seleccionar Figura',
    },
  },
  {
    path: 'profile-select',
    loadComponent: () =>
      import('./profile-select-login.component').then((m) => m.ProfileSelectLoginComponent),
    data: {
      title: 'Confirmar Perfil',
    },
  },
];
