export const UserRoles = {
  Admin: 'Admin',
  Professional: 'Professional',
  FamilyRepresentative: 'FamilyRepresentative',
  PersonWithDisability: 'PersonWithDisability'
};

export type UserRole = string;

export const RoleRoutes: { [key: string]: string } = {
  'Admin': '/admin',
  'Professional': '/pro',
  'FamilyRepresentative': '/family',
  'PersonWithDisability': '/app'
};
