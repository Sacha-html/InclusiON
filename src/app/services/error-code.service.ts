import { Injectable } from '@angular/core';
import { ErrorCode, ErrorInfo, ErrorSeverity } from '../models/error-code.enum';

/**
 * Servicio para mapear códigos de error del backend a mensajes amigables.
 * Centraliza el manejo de errores y proporciona mensajes consistentes en español.
 */
@Injectable({
  providedIn: 'root'
})
export class ErrorCodeService {

  private readonly errorMappings: Map<ErrorCode, Omit<ErrorInfo, 'code'>> = new Map([
    // General (1xx)
    [ErrorCode.None, {
      message: '',
      severity: 'info'
    }],
    [ErrorCode.Unknown, {
      message: 'Ha ocurrido un error inesperado',
      userAction: 'Por favor, intenta de nuevo más tarde',
      severity: 'error'
    }],
    [ErrorCode.InternalError, {
      message: 'Error interno del servidor',
      userAction: 'Por favor, intenta de nuevo más tarde. Si el problema persiste, contacta a soporte',
      severity: 'error'
    }],

    // Validacion (2xx)
    [ErrorCode.ValidationFailed, {
      message: 'Los datos ingresados no son válidos',
      userAction: 'Revisa los campos marcados y corrige los errores',
      severity: 'warning'
    }],
    [ErrorCode.InvalidInput, {
      message: 'Entrada inválida',
      userAction: 'Verifica que los datos ingresados sean correctos',
      severity: 'warning'
    }],
    [ErrorCode.InvalidFormat, {
      message: 'Formato inválido',
      userAction: 'Verifica el formato del dato ingresado',
      severity: 'warning'
    }],
    [ErrorCode.RequiredField, {
      message: 'Campo requerido',
      userAction: 'Completa todos los campos obligatorios',
      severity: 'warning'
    }],
    [ErrorCode.OutOfRange, {
      message: 'Valor fuera de rango',
      userAction: 'Ingresa un valor dentro del rango permitido',
      severity: 'warning'
    }],

    // Autenticacion (3xx)
    [ErrorCode.Unauthorized, {
      message: 'No autorizado',
      userAction: 'Inicia sesión para continuar',
      severity: 'warning'
    }],
    [ErrorCode.InvalidCredentials, {
      message: 'Credenciales incorrectas',
      userAction: 'Verifica tu usuario y contraseña',
      severity: 'error'
    }],
    [ErrorCode.TokenExpired, {
      message: 'Tu sesión ha expirado',
      userAction: 'Por favor, inicia sesión nuevamente',
      severity: 'warning'
    }],
    [ErrorCode.TokenInvalid, {
      message: 'Sesión inválida',
      userAction: 'Por favor, inicia sesión nuevamente',
      severity: 'warning'
    }],
    [ErrorCode.AccountLocked, {
      message: 'Cuenta bloqueada temporalmente',
      userAction: 'Demasiados intentos fallidos. Espera unos minutos e intenta de nuevo',
      severity: 'error'
    }],
    [ErrorCode.AccountInactive, {
      message: 'Cuenta desactivada',
      userAction: 'Contacta al administrador para reactivar tu cuenta',
      severity: 'error'
    }],
    [ErrorCode.TwoFactorRequired, {
      message: 'Se requiere verificación de dos factores',
      userAction: 'Ingresa el código de verificación',
      severity: 'info'
    }],

    // Autorizacion (4xx)
    [ErrorCode.Forbidden, {
      message: 'Acceso denegado',
      userAction: 'No tienes permisos para realizar esta acción',
      severity: 'error'
    }],
    [ErrorCode.InsufficientPermissions, {
      message: 'Permisos insuficientes',
      userAction: 'Contacta al administrador si necesitas acceso',
      severity: 'error'
    }],
    [ErrorCode.NotAuthorizedForResource, {
      message: 'No autorizado para este recurso',
      userAction: 'No tienes acceso a este contenido',
      severity: 'error'
    }],

    // Recursos (5xx)
    [ErrorCode.NotFound, {
      message: 'Recurso no encontrado',
      userAction: 'El elemento que buscas no existe o fue eliminado',
      severity: 'warning'
    }],
    [ErrorCode.UserNotFound, {
      message: 'Usuario no encontrado',
      userAction: 'Verifica que el usuario exista en el sistema',
      severity: 'warning'
    }],
    [ErrorCode.PersonNotFound, {
      message: 'Persona no encontrada',
      userAction: 'Verifica que la persona esté registrada',
      severity: 'warning'
    }],
    [ErrorCode.ResourceNotFound, {
      message: 'Recurso no encontrado',
      userAction: 'El elemento solicitado no existe',
      severity: 'warning'
    }],
    [ErrorCode.ProfessionalNotFound, {
      message: 'Profesional no encontrado',
      userAction: 'El profesional solicitado no existe o fue dado de baja',
      severity: 'warning'
    }],
    [ErrorCode.ReportNotFound, {
      message: 'Informe no encontrado',
      userAction: 'El informe solicitado no existe o fue eliminado',
      severity: 'warning'
    }],

    // Conflictos (6xx)
    [ErrorCode.Conflict, {
      message: 'Conflicto de datos',
      userAction: 'Los datos ya existen o hay un conflicto con información existente',
      severity: 'warning'
    }],
    [ErrorCode.DuplicateEntry, {
      message: 'Registro duplicado',
      userAction: 'Ya existe un registro con estos datos',
      severity: 'warning'
    }],
    [ErrorCode.DocumentAlreadyExists, {
      message: 'Documento ya registrado',
      userAction: 'Ya existe una persona con este número de documento',
      severity: 'warning'
    }],
    [ErrorCode.EmailAlreadyExists, {
      message: 'Email ya registrado',
      userAction: 'Este correo electrónico ya está en uso. ¿Olvidaste tu contraseña?',
      severity: 'warning'
    }],
    [ErrorCode.UsernameAlreadyExists, {
      message: 'Usuario ya existe',
      userAction: 'Este nombre de usuario ya está en uso',
      severity: 'warning'
    }],

    // Negocio (7xx)
    [ErrorCode.BusinessRuleViolation, {
      message: 'Operación no permitida',
      userAction: 'Esta acción viola las reglas del sistema',
      severity: 'error'
    }],
    [ErrorCode.InvalidOperation, {
      message: 'Operación inválida',
      userAction: 'Esta acción no puede realizarse en el estado actual',
      severity: 'error'
    }],
    [ErrorCode.PinNotConfigured, {
      message: 'PIN no configurado',
      userAction: 'Debes configurar un PIN antes de usar este método de acceso',
      severity: 'warning'
    }],
    [ErrorCode.SupervisorNotAuthorized, {
      message: 'Supervisor no autorizado',
      userAction: 'El supervisor seleccionado no tiene permisos para esta acción',
      severity: 'error'
    }],
    [ErrorCode.LoginMethodNotAllowed, {
      message: 'Método de acceso no permitido',
      userAction: 'Este método de inicio de sesión no está disponible para tu cuenta',
      severity: 'error'
    }],
    [ErrorCode.RoleNotAllowedForLogin, {
      message: 'No tienes permisos para acceder desde este portal',
      userAction: 'Usa el portal correspondiente a tu tipo de cuenta',
      severity: 'error'
    }],
    [ErrorCode.CannotDeactivateSelf, {
      message: 'No puedes desactivar tu propia cuenta',
      severity: 'error'
    }],
    [ErrorCode.UserAlreadyInactive, {
      message: 'El usuario ya se encuentra inactivo',
      severity: 'warning'
    }],
    [ErrorCode.UserAlreadyActive, {
      message: 'El usuario ya se encuentra activo',
      severity: 'warning'
    }],
    [ErrorCode.ProfessionalNotApproved, {
      message: 'Profesional pendiente de aprobación',
      userAction: 'Tu cuenta profesional aún no fue aprobada. Contactá al administrador',
      severity: 'warning'
    }],
  ]);

  /** Mensaje por defecto para códigos no mapeados */
  private readonly defaultError: Omit<ErrorInfo, 'code'> = {
    message: 'Ha ocurrido un error',
    userAction: 'Por favor, intenta de nuevo',
    severity: 'error'
  };

  /**
   * Obtiene la información completa del error a partir del código
   */
  getErrorInfo(code: ErrorCode | number | undefined): ErrorInfo {
    const errorCode = this.normalizeCode(code);
    const mapping = this.errorMappings.get(errorCode) ?? this.defaultError;

    return {
      code: errorCode,
      ...mapping
    };
  }

  /**
   * Obtiene solo el mensaje de error
   */
  getMessage(code: ErrorCode | number | undefined): string {
    return this.getErrorInfo(code).message;
  }

  /**
   * Obtiene el mensaje con la acción sugerida
   */
  getFullMessage(code: ErrorCode | number | undefined): string {
    const info = this.getErrorInfo(code);
    return info.userAction
      ? `${info.message}. ${info.userAction}`
      : info.message;
  }

  /**
   * Obtiene la severidad del error
   */
  getSeverity(code: ErrorCode | number | undefined): ErrorSeverity {
    return this.getErrorInfo(code).severity;
  }

  /**
   * Verifica si el error requiere re-autenticación
   */
  requiresReauth(code: ErrorCode | number | undefined): boolean {
    const errorCode = this.normalizeCode(code);
    return [
      ErrorCode.Unauthorized,
      ErrorCode.TokenExpired,
      ErrorCode.TokenInvalid,
      ErrorCode.AccountInactive
    ].includes(errorCode);
  }

  /**
   * Verifica si es un error de validación
   */
  isValidationError(code: ErrorCode | number | undefined): boolean {
    const errorCode = this.normalizeCode(code);
    return errorCode >= 200 && errorCode < 300;
  }

  /**
   * Verifica si es un error de autenticación
   */
  isAuthError(code: ErrorCode | number | undefined): boolean {
    const errorCode = this.normalizeCode(code);
    return errorCode >= 300 && errorCode < 400;
  }

  /**
   * Verifica si es un error de conflicto/duplicado
   */
  isConflictError(code: ErrorCode | number | undefined): boolean {
    const errorCode = this.normalizeCode(code);
    return errorCode >= 600 && errorCode < 700;
  }

  /**
   * Normaliza el código a ErrorCode enum
   */
  private normalizeCode(code: ErrorCode | number | undefined): ErrorCode {
    if (code === undefined || code === null) {
      return ErrorCode.Unknown;
    }
    // Verificar si es un valor válido del enum
    if (Object.values(ErrorCode).includes(code as ErrorCode)) {
      return code as ErrorCode;
    }
    return ErrorCode.Unknown;
  }
}
