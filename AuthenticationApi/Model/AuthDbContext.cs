using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationApi.Model {
    public class AuthDbContext : IdentityDbContext<IdentityUser> {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) {
        }
    }
}
