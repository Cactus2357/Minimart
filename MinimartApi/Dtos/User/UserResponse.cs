using MinimartApi.Dtos.Authentication;

namespace MinimartApi.Dtos.User {
    public class UserResponse {
        public string UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public required bool IsEmailConfirmed { get; set; }
        //public string? FullName { get; set; } = string.Empty;
        //public string? Address { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public IList<string>? Roles { get; set; }
        public IList<AddressResponse>? Addresses { get; set; }
    }
}
