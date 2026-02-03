namespace InclusiON.DTOs.Common
{
    /// <summary>
    /// Resultado de una operacion que puede ser exitosa o fallida.
    /// Implementa el Result Pattern para manejo explicito de errores.
    /// </summary>
    public class Result<T>
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public T? Value { get; }
        public Error Error { get; }

        private Result(T? value, bool isSuccess, Error error)
        {
            Value = value;
            IsSuccess = isSuccess;
            Error = error;
        }

        /// <summary>
        /// Crea un resultado exitoso con valor.
        /// </summary>
        public static Result<T> Success(T value) => new(value, true, Error.None);

        /// <summary>
        /// Crea un resultado fallido con error.
        /// </summary>
        public static Result<T> Failure(Error error) => new(default, false, error);

        /// <summary>
        /// Crea un resultado de validacion fallida.
        /// </summary>
        public static Result<T> ValidationFailure(string message, Dictionary<string, string[]>? fieldErrors = null)
            => Failure(Error.Validation(message, fieldErrors));

        /// <summary>
        /// Crea un resultado de recurso no encontrado.
        /// </summary>
        public static Result<T> NotFound(string resource = "Recurso")
            => Failure(Error.NotFound(resource));

        /// <summary>
        /// Crea un resultado de no autorizado.
        /// </summary>
        public static Result<T> Unauthorized(string message = "No autorizado")
            => Failure(Error.Unauthorized(message));

        /// <summary>
        /// Crea un resultado de acceso prohibido.
        /// </summary>
        public static Result<T> Forbidden(string message = "Acceso denegado")
            => Failure(Error.Forbidden(message));

        /// <summary>
        /// Operador implicito para convertir valor a Result exitoso.
        /// </summary>
        public static implicit operator Result<T>(T value) => Success(value);

        /// <summary>
        /// Operador implicito para convertir Error a Result fallido.
        /// </summary>
        public static implicit operator Result<T>(Error error) => Failure(error);

        /// <summary>
        /// Ejecuta una accion si el resultado es exitoso.
        /// </summary>
        public Result<T> OnSuccess(Action<T> action)
        {
            if (IsSuccess && Value is not null)
            {
                action(Value);
            }
            return this;
        }

        /// <summary>
        /// Ejecuta una accion si el resultado es fallido.
        /// </summary>
        public Result<T> OnFailure(Action<Error> action)
        {
            if (IsFailure)
            {
                action(Error);
            }
            return this;
        }

        /// <summary>
        /// Transforma el valor si es exitoso.
        /// </summary>
        public Result<TNew> Map<TNew>(Func<T, TNew> mapper)
        {
            return IsSuccess && Value is not null
                ? Result<TNew>.Success(mapper(Value))
                : Result<TNew>.Failure(Error);
        }

        /// <summary>
        /// Encadena otra operacion si es exitoso.
        /// </summary>
        public async Task<Result<TNew>> Bind<TNew>(Func<T, Task<Result<TNew>>> func)
        {
            return IsSuccess && Value is not null
                ? await func(Value)
                : Result<TNew>.Failure(Error);
        }

        /// <summary>
        /// Obtiene el valor o un valor por defecto.
        /// </summary>
        public T? GetValueOrDefault(T? defaultValue = default) => IsSuccess ? Value : defaultValue;

        /// <summary>
        /// Patron Match para manejar ambos casos.
        /// </summary>
        public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<Error, TResult> onFailure)
        {
            return IsSuccess && Value is not null ? onSuccess(Value) : onFailure(Error);
        }
    }

    /// <summary>
    /// Resultado sin valor (para operaciones void).
    /// </summary>
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public Error Error { get; }

        private Result(bool isSuccess, Error error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Success() => new(true, Error.None);
        public static Result Failure(Error error) => new(false, error);

        public static implicit operator Result(Error error) => Failure(error);
    }
}
