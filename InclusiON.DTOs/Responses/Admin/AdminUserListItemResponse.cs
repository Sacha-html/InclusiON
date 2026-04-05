using System;
using System.Data;

namespace InclusiON.DTOs.Responses.Admin
{
    public class AdminUserListItemResponse
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool MustChangePassword { get; set; }

        public static AdminUserListItemResponse FromReader(IDataReader reader)
        {
            return new AdminUserListItemResponse
            {
                UserId = reader.GetGuid(reader.GetOrdinal("UserId")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                LastLoginDate = reader.IsDBNull(reader.GetOrdinal("LastLoginDate"))
                    ? null
                    : reader.GetDateTime(reader.GetOrdinal("LastLoginDate")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                MustChangePassword = reader.GetBoolean(reader.GetOrdinal("MustChangePassword")),
                Role = reader.GetString(reader.GetOrdinal("Role")),
                FullName = reader.GetString(reader.GetOrdinal("FullName"))
            };
        }
    }
}
