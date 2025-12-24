using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimartApi.Dtos;
using MinimartApi.Models;
using System.Security.Claims;

namespace MinimartApi.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = Role.Admin)]
    public class UsersController : ControllerBase {
        private readonly AppDbContext context;
        private readonly IPasswordHasher<User> passwordHasher;

        public UsersController(AppDbContext context, IPasswordHasher<User> passwordHasher) {
            this.context = context;
            this.passwordHasher = passwordHasher;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers(bool includeDeleted = false) {
            var q = context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .AsQueryable();

            if (includeDeleted)
                q = q.IgnoreQueryFilters();

            //TODO: add pagination

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

        //TODO: search user by username or email

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserById(int userId) {
            var user = await context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Where(u => u.UserId == userId)
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
                .FirstOrDefaultAsync();
            if (user == null) {
                return NotFound(new { Message = "User not found." });
            }
            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request) {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var username = request.Username.Trim().ToLower();
            var email = request.Email.Trim().ToLowerInvariant();

            var usernameExists = await context.Users.AnyAsync(u => u.Username.ToLower() == username);
            var emailExists = await context.Users.AnyAsync(u => u.Email.ToLower() == email);

            if (emailExists == true)
                ModelState.AddModelError(nameof(request.Email), $"Email '{email}' is already registered.");

            if (usernameExists == true)
                ModelState.AddModelError(nameof(request.Username), $"Username '{username}' is already taken.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            var user = new User {
                Username = username,
                Email = email,
            };

            user.PasswordHash = passwordHasher.HashPassword(user, "123456"); //TODO: handle password properly

            user.FullName = request.FullName?.Trim();
            user.Address = request.Address?.Trim();

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUserById), new { userId = user.UserId }, new { user.UserId, user.Username, user.Email, user.CreatedAt });
        }

        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUser(int userId, [FromBody] UpdateUserRequest request) {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await context.Users.FindAsync(userId);
            if (user == null) {
                return NotFound(new { Message = "User not found." });
            }

            if (request.Password != null) {
                user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
            }

            user.FullName = request.FullName;
            user.Address = request.Address;

            context.Users.Update(user);
            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser(int userId) {
            var user = await context.Users.FindAsync(userId);
            
            if (user == null) {
                return NotFound(new { Message = "User not found." });
            }

            context.Users.Remove(user);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{userId}/restore")]
        public async Task<IActionResult> RestoreUser(int userId) {
            var user = await context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.UserId == userId && u.IsDeleted);

            if (user == null)
                return NotFound(new { Message = "User not found or is not deleted." });

            var emailExists = await context.Users
                .AnyAsync(u => !u.IsDeleted && u.Email == user.Email && u.UserId != userId);

            if (emailExists == true)
                return Conflict(new { Message = "Cannot restore user. Another active user with the same email already exists." });

            var usernameExists = await context.Users
                .AnyAsync(u => !u.IsDeleted && u.Username == user.Username && u.UserId != userId);

            if (usernameExists == true)
                return Conflict(new { Message = "Cannot restore user. Another active user with the same username already exists." });

            user.IsEmailConfirmed = false;
            user.IsDeleted = false;
            user.DeletedAt = null;

            await context.SaveChangesAsync();

            return NoContent();
        }
    }
}
