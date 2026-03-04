namespace InclusiON.Application.Exceptions
{
    /// <summary>
    /// Excepción base para errores de repositorio.
    /// </summary>
    public class RepositoryException : Exception
    {
        public string? EntityType { get; }
        public object? EntityId { get; }

        public RepositoryException(string message) : base(message)
        {
        }

        public RepositoryException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public RepositoryException(string message, string entityType, object? entityId = null)
            : base(message)
        {
            EntityType = entityType;
            EntityId = entityId;
        }

        public RepositoryException(string message, string entityType, object? entityId, Exception innerException)
            : base(message, innerException)
        {
            EntityType = entityType;
            EntityId = entityId;
        }
    }

    /// <summary>
    /// Excepción cuando no se encuentra una entidad.
    /// </summary>
    public class EntityNotFoundException : RepositoryException
    {
        public EntityNotFoundException(string entityType, object entityId)
            : base($"{entityType} with ID '{entityId}' was not found.", entityType, entityId)
        {
        }

        public EntityNotFoundException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Excepción para errores de acceso a datos.
    /// </summary>
    public class DataAccessException : RepositoryException
    {
        public DataAccessException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public DataAccessException(string message, string entityType, Exception innerException)
            : base(message, entityType, null, innerException)
        {
        }
    }
}
