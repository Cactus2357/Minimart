using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MinimartApi.Models {

    public class User {
        public int UserId { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public bool IsEmailConfirmed { get; set; } = false;
        public string? PasswordHash { get; set; } = null;

        public string? FullName { get; set; } = string.Empty;
        public string? Address { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public virtual ICollection<Sale> Sales { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}
