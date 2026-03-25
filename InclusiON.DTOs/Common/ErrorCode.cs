namespace InclusiON.DTOs.Common
{
    /// <summary>
    /// Codigos de error estandarizados para el frontend.
    /// </summary>
    public enum ErrorCode
    {
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
        ProfessionalNotFound = 504,

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
        CannotDeactivateSelf = 706,
        UserAlreadyInactive = 707,
        UserAlreadyActive = 708,

        // Invitaciones (8xx)
        InvitationNotFound = 800,
        InvitationExpired = 801,
        InvitationAlreadyUsed = 802
    }
}
