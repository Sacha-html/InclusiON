namespace InclusiON.DTOs.Common
{
    /// <summary>
    /// Representa un error con codigo, mensaje y detalles opcionales.
    /// </summary>
    public record Error
    {
        public ErrorCode Code { get; init; }
        public string Message { get; init; }
        public Dictionary<string, string[]>? FieldErrors { get; init; }

        private Error(ErrorCode code, string message, Dictionary<string, string[]>? fieldErrors = null)
        {
            Code = code;
            Message = message;
            FieldErrors = fieldErrors;
        }

        // Factory methods para errores comunes
        public static Error None => new(ErrorCode.None, string.Empty);

        public static Error Validation(string message, Dictionary<string, string[]>? fieldErrors = null)
            => new(ErrorCode.ValidationFailed, message, fieldErrors);

        public static Error Validation(string field, string error)
            => new(ErrorCode.ValidationFailed, error, new Dictionary<string, string[]> { { field, new[] { error } } });

        public static Error NotFound(string resource = "Recurso")
            => new(ErrorCode.NotFound, $"{resource} no encontrado");

        public static Error NotFound(ErrorCode code, string message)
            => new(code, message);

        public static Error Unauthorized(string message = "No autorizado")
            => new(ErrorCode.Unauthorized, message);

        public static Error InvalidCredentials(string message = "Credenciales invalidas")
            => new(ErrorCode.InvalidCredentials, message);

        public static Error AccountLocked(int? minutesRemaining = null)
            => new(ErrorCode.AccountLocked,
                minutesRemaining.HasValue
                    ? $"Cuenta bloqueada. Intente en {minutesRemaining} minuto(s)"
                    : "Cuenta bloqueada por intentos fallidos");

        public static Error AccountInactive()
            => new(ErrorCode.AccountInactive, "Cuenta inactiva. Contacte a soporte");

        public static Error Forbidden(string message = "Acceso denegado")
            => new(ErrorCode.Forbidden, message);

        public static Error Conflict(string message)
            => new(ErrorCode.Conflict, message);

        public static Error Duplicate(ErrorCode code, string message)
            => new(code, message);

        public static Error BusinessRule(ErrorCode code, string message)
            => new(code, message);

        public static Error Internal(string message = "Error interno del servidor")
            => new(ErrorCode.InternalError, message);

        public static Error Custom(ErrorCode code, string message)
            => new(code, message);
    }
}
