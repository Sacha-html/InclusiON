namespace InclusiON.DTOs.Responses.Roles
{
    public class RoleResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new();
    }
}
