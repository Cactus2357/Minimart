using System.ComponentModel.DataAnnotations;

namespace MinimartApi.Dtos.User {
    public class UserCreateRequest {
        [Required, StringLength(50, MinimumLength = 3)]
        public required string Username { get; set; }

        [Required, EmailAddress]
        public required string Email { get; set; }
        
        public string? Password { get; set; }

        [StringLength(100)]
        public string? FullName { get; set; }
        
        [StringLength(250)]
        public string? Address { get; set; }
    }
}
