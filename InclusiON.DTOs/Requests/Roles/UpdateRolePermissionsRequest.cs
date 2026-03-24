using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Roles
{
    public class UpdateRolePermissionsRequest
    {
        [Required]
        public List<string> Permissions { get; set; } = new();
    }
}
