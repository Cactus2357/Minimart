namespace MinimartApi.Dtos {
    public class UserResponse {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public required bool IsEmailConfirmed { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public IList<string> Roles { get; set; } = new List<string>();
    }
}
