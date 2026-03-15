using System.ComponentModel.DataAnnotations;

namespace MinimartApi.Db.Models
{

    public class User
    {
        [Key]
        public Guid UserId { get; set; }

        [Required, MaxLength(100)]
        public string Username { get; set; }

        [Required, EmailAddress, MaxLength(255)]
        public string Email { get; set; }
        public bool IsEmailConfirmed { get; set; } = false;

        public string? PasswordHash { get; set; } = null;

        public bool IsDeleted { get; set; } = false;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<UserRole> UserRoles { get; set; }
        public ICollection<Address> Addresses { get; set; }
    }
}
