namespace InclusiON.DTOs.Responses
{
    public class ApiResponse<T> where T : class
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        private ApiResponse() { }

        public static ApiResponse<T> SuccessResult(T data, string message = "Success")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
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
                Data = default(T),
                Errors = new List<string>(),
                Timestamp = DateTime.UtcNow
            };
        }

        public static ApiResponse<T> ErrorResult(string message, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default(T),
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
                Data = default(T),
                Errors = new List<string> { singleError },
                Timestamp = DateTime.UtcNow
            };
        }

        public static ApiResponse<T> ValidationError(List<string> validationErrors)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = "Validation failed",
                Data = default(T),
                Errors = validationErrors,
                Timestamp = DateTime.UtcNow
            };
        }

        public static ApiResponse<T> NotFound(string resourceName = "Resource")
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = $"{resourceName} not found",
                Data = default(T),
                Errors = new List<string>(),
                Timestamp = DateTime.UtcNow
            };
        }

        public static ApiResponse<T> Unauthorized(string message = "Unauthorized access")
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default(T),
                Errors = new List<string>(),
                Timestamp = DateTime.UtcNow
            };
        }

        public static ApiResponse<T> Forbidden(string message = "Access forbidden")
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default(T),
                Errors = new List<string>(),
                Timestamp = DateTime.UtcNow
            };
        }
    }
}
