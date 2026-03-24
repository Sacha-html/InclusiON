using Microsoft.AspNetCore.Mvc;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;

namespace InclusiON.Api.Extensions
{
    /// <summary>
    /// Extensiones para convertir Result a ActionResult con el HTTP status code apropiado.
    /// </summary>
    public static class ResultExtensions
    {
        /// <summary>
        /// Convierte un ApiResponse a ActionResult con el status code apropiado segun el ErrorCode.
        /// </summary>
        public static ActionResult<ApiResponse<T>> ToActionResult<T>(this ApiResponse<T> response) where T : class
        {
            if (response.Success)
            {
                return new OkObjectResult(response);
            }

            return response.ErrorCode switch
            {
                // 400 Bad Request - Errores de validacion y entrada
                ErrorCode.ValidationFailed or
                ErrorCode.InvalidInput or
                ErrorCode.InvalidFormat or
                ErrorCode.RequiredField or
                ErrorCode.OutOfRange or
                ErrorCode.BusinessRuleViolation or
                ErrorCode.InvalidOperation or
                ErrorCode.PinNotConfigured or
                ErrorCode.LoginMethodNotAllowed or
                ErrorCode.InvitationExpired or
                ErrorCode.InvitationAlreadyUsed
                    => new BadRequestObjectResult(response),

                // 401 Unauthorized - Errores de autenticacion
                ErrorCode.Unauthorized or
                ErrorCode.InvalidCredentials or
                ErrorCode.TokenExpired or
                ErrorCode.TokenInvalid or
                ErrorCode.AccountLocked or
                ErrorCode.AccountInactive or
                ErrorCode.TwoFactorRequired
                    => new UnauthorizedObjectResult(response),

                // 403 Forbidden - Errores de autorizacion
                ErrorCode.Forbidden or
                ErrorCode.InsufficientPermissions or
                ErrorCode.NotAuthorizedForResource or
                ErrorCode.SupervisorNotAuthorized
                    => new ObjectResult(response) { StatusCode = StatusCodes.Status403Forbidden },

                // 404 Not Found - Recursos no encontrados
                ErrorCode.NotFound or
                ErrorCode.UserNotFound or
                ErrorCode.PersonNotFound or
                ErrorCode.ResourceNotFound or
                ErrorCode.ProfessionalNotFound or
                ErrorCode.InvitationNotFound
                    => new NotFoundObjectResult(response),

                // 409 Conflict - Conflictos de datos
                ErrorCode.Conflict or
                ErrorCode.DuplicateEntry or
                ErrorCode.DocumentAlreadyExists or
                ErrorCode.EmailAlreadyExists or
                ErrorCode.UsernameAlreadyExists
                    => new ConflictObjectResult(response),

                // 500 Internal Server Error - Errores internos
                ErrorCode.InternalError or
                ErrorCode.Unknown or
                _ => new ObjectResult(response) { StatusCode = StatusCodes.Status500InternalServerError }
            };
        }

        /// <summary>
        /// Convierte un Result a ActionResult con ApiResponse.
        /// </summary>
        public static ActionResult<ApiResponse<T>> ToActionResult<T>(
            this Result<T> result,
            string successMessage = "Operacion exitosa") where T : class
        {
            var response = ApiResponse<T>.FromResult(result, successMessage);
            return response.ToActionResult();
        }

        /// <summary>
        /// Convierte un Result a ActionResult con ApiResponse y status 201 Created.
        /// </summary>
        public static ActionResult<ApiResponse<T>> ToCreatedActionResult<T>(
            this Result<T> result,
            string actionName,
            object routeValues,
            string successMessage = "Recurso creado exitosamente") where T : class
        {
            if (result.IsFailure)
            {
                var errorResponse = ApiResponse<T>.FromResult(result);
                return errorResponse.ToActionResult();
            }

            var response = ApiResponse<T>.FromResult(result, successMessage);
            return new CreatedAtActionResult(actionName, null, routeValues, response);
        }
    }
}
