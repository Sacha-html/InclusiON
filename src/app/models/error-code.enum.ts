/**
 * Codigos de error estandarizados sincronizados con el backend.
 * Debe mantenerse en sincronía con InclusiON.DTOs.Common.ErrorCode
 */
export enum ErrorCode {
  // General (1xx)
  None = 0,
  Unknown = 100,
  InternalError = 101,

  // Validacion (2xx)
  ValidationFailed = 200,
  InvalidInput = 201,
  InvalidFormat = 202,
  RequiredField = 203,
  OutOfRange = 204,

  // Autenticacion (3xx)
  Unauthorized = 300,
  InvalidCredentials = 301,
  TokenExpired = 302,
  TokenInvalid = 303,
  AccountLocked = 304,
  AccountInactive = 305,
  TwoFactorRequired = 306,

  // Autorizacion (4xx)
  Forbidden = 400,
  InsufficientPermissions = 401,
  NotAuthorizedForResource = 402,

  // Recursos (5xx)
  NotFound = 500,
  UserNotFound = 501,
  PersonNotFound = 502,
  ResourceNotFound = 503,

  // Conflictos (6xx)
  Conflict = 600,
  DuplicateEntry = 601,
  DocumentAlreadyExists = 602,
  EmailAlreadyExists = 603,
  UsernameAlreadyExists = 604,

  // Negocio (7xx)
  BusinessRuleViolation = 700,
  InvalidOperation = 701,
  PinNotConfigured = 702,
  SupervisorNotAuthorized = 703,
  LoginMethodNotAllowed = 704,
  RoleNotAllowedForLogin = 705,

  // Invitaciones (8xx)
  InvitationNotFound = 800,
  InvitationExpired = 801,
  InvitationAlreadyUsed = 802
}

/** Severidad del error para UI */
export type ErrorSeverity = 'error' | 'warning' | 'info';

/** Información completa de un error mapeado */
export interface ErrorInfo {
  code: ErrorCode;
  message: string;
  userAction?: string;
  severity: ErrorSeverity;
}
