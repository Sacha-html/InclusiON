namespace InclusiON.Application.Authorization
{
    /// <summary>
    /// Modo de acceso evaluado por <c>IResourceAuthorizationService</c>.
    /// Algunos vinculos pueden permitir lectura pero no escritura (ej. profesional de
    /// otra institucion con acceso de solo lectura a un diagnostico).
    /// </summary>
    public enum AccessMode
    {
        Read = 0,
        Write = 1
    }
}
