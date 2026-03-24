namespace InclusiON.DTOs.Responses
{
    /// <summary>
    /// Respuesta al crear un nuevo usuario administrador.
    /// </summary>
    public class CreateAdminUserResponse
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int InstitutionId { get; set; }
        public string InstitutionName { get; set; } = string.Empty;
        public string TemporaryPassword { get; set; } = string.Empty;
    }
}
