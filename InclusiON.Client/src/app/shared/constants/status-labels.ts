/**
 * Etiquetas de estado para mostrar en la UI.
 * Usar en lugar de strings literales en badges, switches y comparaciones.
 */
export const AssignmentStatus = {
  Pendiente:  'Pendiente',
  EnProgreso: 'En Progreso',
  Completada: 'Completada',
  Cancelada:  'Cancelada',
} as const;

export const ReportStatus = {
  Borrador:  'Borrador',
  Enviado:   'Pendiente',
  Aprobado:  'Aprobado',
  Rechazado: 'Rechazado',
} as const;

export const ActiveStatus = {
  Activo:   'Activo',
  Inactivo: 'Inactivo',
} as const;

export const ValidationStatus = {
  Pendiente: 'Pendiente',
  Aprobado:  'Aprobado',
  Rechazado: 'Rechazado',
} as const;
