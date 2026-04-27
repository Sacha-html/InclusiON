namespace InclusiON.DTOs.Auth
{
    public class TokenUserData
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public List<string> Permissions { get; set; } = new();
        public bool IsGlobalAdmin { get; set; }
        public List<int> InstitutionIds { get; set; } = new();
        /// <summary>
        /// ID del entity de dominio: professionalId, familyRepresentativeId o personId según el rol.
        /// Null para Admin (usa InstitutionIds) y GlobalAdmin.
        /// Se embebe en el JWT encriptado con AES-GCM bajo el claim "eid".
        /// </summary>
        public Guid? EntityId { get; set; }

        public TokenUserData()
        {

        }

        public TokenUserData(Guid id, string name, string email, string role, bool isActive, List<string> permissions)
        {
            Id = id;
            Name = name;
            Email = email;
            Role = role;
            IsActive = isActive;
            Permissions = permissions;
        }
    }
}
