using InclusiON.DTOs.Common;
using InclusiON.Shared.Resources;

namespace InclusiON.DTOs.Responses
{
    public class ApiResponse<T> where T : class
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public ErrorCode ErrorCode { get; set; } = ErrorCode.None;
        public List<string> Errors { get; set; } = new();
        public Dictionary<string, string[]>? FieldErrors { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        private ApiResponse() { }

        #region Success Methods

        public static ApiResponse<T> SuccessResult(T data, string message = "Success")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                ErrorCode = ErrorCode.None,
                Errors = new List<string>(),
                Timestamp = DateTime.UtcNow
            };
        }

        public static ApiResponse<T> SuccessResult(string message = "Success")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = default,
                ErrorCode = ErrorCode.None,
                Errors = new List<string>(),
                Timestamp = DateTime.UtcNow
            };
        }

        #endregion

        #region Error Methods

        public static ApiResponse<T> ErrorResult(string message, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default,
                ErrorCode = ErrorCode.Unknown,
                Errors = errors ?? new List<string>(),
                Timestamp = DateTime.UtcNow
            };
        }

        public static ApiResponse<T> ErrorResult(string message, string singleError)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default,
                ErrorCode = ErrorCode.Unknown,
                Errors = new List<string> { singleError },
                Timestamp = DateTime.UtcNow
            };
        }

        public static ApiResponse<T> ErrorResult(ErrorCode code, string message, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default,
                ErrorCode = code,
                Errors = errors ?? new List<string>(),
                Timestamp = DateTime.UtcNow
            };
        }

        #endregion

        #region Typed Error Methods

        public static ApiResponse<T> ValidationError(List<string> validationErrors)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = ErrorMessages.ValidationFailed,
                Data = default,
                ErrorCode = ErrorCode.ValidationFailed,
                Errors = validationErrors,
                Timestamp = DateTime.UtcNow
            };
        }

        public static ApiResponse<T> ValidationError(Dictionary<string, string[]> fieldErrors)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = ErrorMessages.ValidationFailed,
                Data = default,
                ErrorCode = ErrorCode.ValidationFailed,
                Errors = new List<string>(),
                FieldErrors = fieldErrors,
                Timestamp = DateTime.UtcNow
            };
        }

        public static ApiResponse<T> NotFound(string resourceName = "Recurso")
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = string.Format(ErrorMessages.ResourceNotFound, resourceName),
                Data = default,
                ErrorCode = ErrorCode.NotFound,
                Errors = new List<string>(),
                Timestamp = DateTime.UtcNow
            };
        }

        public static ApiResponse<T> Unauthorized(string? message = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message ?? ErrorMessages.NotAuthorized,
                Data = default,
                ErrorCode = ErrorCode.Unauthorized,
                Errors = new List<string>(),
                Timestamp = DateTime.UtcNow
            };
        }

        public static ApiResponse<T> Forbidden(string? message = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message ?? ErrorMessages.AccessDenied,
                Data = default,
                ErrorCode = ErrorCode.Forbidden,
                Errors = new List<string>(),
                Timestamp = DateTime.UtcNow
            };
        }

        public static ApiResponse<T> AccountLocked(int? minutesRemaining = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = minutesRemaining.HasValue
                    ? string.Format(ErrorMessages.AccountLockedMinutes, minutesRemaining)
                    : ErrorMessages.AccountLocked,
                Data = default,
                ErrorCode = ErrorCode.AccountLocked,
                Errors = new List<string>(),
                Timestamp = DateTime.UtcNow
            };
        }

        public static ApiResponse<T> Conflict(ErrorCode code, string message)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default,
                ErrorCode = code,
                Errors = new List<string>(),
                Timestamp = DateTime.UtcNow
            };
        }

        #endregion

        #region Result Pattern Integration

        /// <summary>
        /// Crea un ApiResponse desde un Result.
        /// </summary>
        public static ApiResponse<T> FromResult(Result<T> result, string successMessage = "Success")
        {
            if (result.IsSuccess)
            {
                return SuccessResult(result.Value!, successMessage);
            }

            return new ApiResponse<T>
            {
                Success = false,
                Message = result.Error.Message,
                Data = default,
                ErrorCode = result.Error.Code,
                Errors = new List<string>(),
                FieldErrors = result.Error.FieldErrors,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Crea un ApiResponse de error desde un Error.
        /// </summary>
        public static ApiResponse<T> FromError(Error error)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = error.Message,
                Data = default,
                ErrorCode = error.Code,
                Errors = new List<string>(),
                FieldErrors = error.FieldErrors,
                Timestamp = DateTime.UtcNow
            };
        }

        #endregion
    }
}
