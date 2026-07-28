/**
 * Constantes de permisos del sistema.
 * Usar en hasPermission(), route data y guards en lugar de strings literales.
 * Deben coincidir exactamente con los claims de permisos en el JWT.
 */
export const Permissions = {
  Users: {
    Read:   'users:read',
    Create: 'users:create',
    Update: 'users:update',
    Delete: 'users:delete',
  },
  Persons: {
    Read:   'persons:read',
    Create: 'persons:create',
    Update: 'persons:update',
    Delete: 'persons:delete',
  },
  Professionals: {
    Read:         'professionals:read',
    Create:       'professionals:create',
    Update:       'professionals:update',
    Delete:       'professionals:delete',
    LinkFamily:   'professionals:link-family',
    UnlinkFamily: 'professionals:unlink-family',
  },
  Family: {
    Read:   'family:read',
    Create: 'family:create',
    Update: 'family:update',
    Delete: 'family:delete',
    Link:   'family:link',
    Unlink: 'family:unlink',
  },
  Activities: {
    Read:    'activities:read',
    Create:  'activities:create',
    Update:  'activities:update',
    Delete:  'activities:delete',
    Respond: 'activities:respond',
  },
  Diagnoses: {
    Read:   'diagnoses:read',
    Create: 'diagnoses:create',
    Update: 'diagnoses:update',
  },
  Roadmap: {
    Read:   'roadmap:read',
    Create: 'roadmap:create',
    Update: 'roadmap:update',
    Delete: 'roadmap:delete',
  },
  Reports: {
    Read:    'reports:read',
    Create:  'reports:create',
    Submit:  'reports:submit',
    Approve: 'reports:approve',
    Reject:  'reports:reject',
    Export:  'reports:export',
  },
  Messages: {
    Read:   'messages:read',
    Create: 'messages:create',
  },
  Invitations: {
    Read:   'invitations:read',
    Create: 'invitations:create',
  },
  Institutions: {
    Read:   'institutions:read',
    Create: 'institutions:create',
    Update: 'institutions:update',
  },
  Settings: {
    Read:   'settings:read',
    Update: 'settings:update',
  },
  Audit: {
    Read: 'audit:read',
  },
} as const;
