using Microsoft.AspNetCore.Identity;

namespace MinimartApi.Models {
    public class User : IdentityUser {
        public string Username { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}
