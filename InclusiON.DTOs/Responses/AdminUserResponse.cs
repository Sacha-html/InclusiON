namespace InclusiON.DTOs.Responses
{
    /// <summary>
    /// Respuesta con informacion de un usuario administrador y sus instituciones asignadas.
    /// </summary>
    public class AdminUserResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string FullName => $"{Name} {Surname}".Trim();
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsGlobalAdmin { get; set; }
        public List<AdminInstitutionInfo> Institutions { get; set; } = new();
    }

    /// <summary>
    /// Informacion basica de una institucion asignada a un administrador.
    /// </summary>
    public class AdminInstitutionInfo
    {
        public int InstitutionId { get; set; }
        public string InstitutionName { get; set; } = string.Empty;
    }
}
