using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Admin
{
    public class UpdateAdminUserRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Surname { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;
    }
}
