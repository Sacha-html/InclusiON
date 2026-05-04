/**
 * Nombres de roles del sistema tal como viajan en el JWT claim "role".
 * Usar en hasRole(), hasAnyRole() y route data en lugar de strings literales.
 * Deben coincidir con los roles registrados en ASP.NET Core Identity.
 */
export const UserRoles = {
  Admin:                'Admin',
  Professional:         'Professional',
  FamilyRepresentative: 'FamilyRepresentative',
  PersonWithDisability: 'PersonWithDisability',
  /** Tipo de usuario en el flujo de login visual asistido (no es rol de Identity). */
  Family:               'Family',
  /** Abreviatura usada en el flujo visual login para PersonWithDisability. */
  Person:               'Person',
} as const;

export type UserRole = string;

export const RoleRoutes: Record<string, string> = {
  [UserRoles.Admin]:                '/admin',
  [UserRoles.Professional]:         '/pro',
  [UserRoles.FamilyRepresentative]: '/family',
  [UserRoles.PersonWithDisability]: '/app',
};
