using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MinimartApi.Models {

    public class User {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string? PasswordHash { get; set; }

        public string FullName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public virtual ICollection<Sale> Sales { get; set; }
    }
}
