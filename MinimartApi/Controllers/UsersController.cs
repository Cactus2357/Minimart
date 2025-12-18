using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimartApi.Dtos;
using MinimartApi.Models;

namespace MinimartApi.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase {
        private readonly AppDbContext context;

        public UsersController(AppDbContext context) {
            this.context = context;
        }

        [HttpGet("all")]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> GetAllUsers(bool includeDeleted = false) {
            var q = context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .AsQueryable();

            if (includeDeleted)
                q = q.IgnoreQueryFilters();

            var users = await q
                .Select(u => new UserResponse {
                    UserId = u.UserId,
                    Username = u.Username,
                    Email = u.Email,
                    IsEmailConfirmed = u.IsEmailConfirmed,
                    Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList(),
                    FullName = u.FullName,
                    Address = u.Address,
                    CreatedAt = u.CreatedAt,
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpDelete("{userId}")]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> DeleteUser(int userId) {
            var user = await context.Users.FindAsync(userId);
            if (user == null) {
                return NotFound(new { Message = "User not found." });
            }
            context.Users.Remove(user);
            await context.SaveChangesAsync();
            return Ok();
        }
    }
}
