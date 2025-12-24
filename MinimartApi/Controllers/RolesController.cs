using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimartApi.Dtos;
using MinimartApi.Models;

namespace MinimartApi.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = Role.Admin)]
    public class RolesController : ControllerBase {
        private readonly AppDbContext context;

        public RolesController(AppDbContext context) {
            this.context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRoles(bool includeDeleted = false) {
            var q = context.Roles.AsQueryable();

            if (includeDeleted)
                q = q.IgnoreQueryFilters();

            return Ok(await q.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request) {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await context.Roles.AnyAsync(r => r.Name.ToLower() == request.Name.ToLower()))
                ModelState.AddModelError(nameof(request.Name), $"Role '{request.Name.ToLower()}' already exists.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var role = new Role {
                Name = request.Name.Trim()
            };

            await context.Roles.AddAsync(role);
            await context.SaveChangesAsync();

            return Ok(role);
        }

        [HttpPut("{roleId}")]
        public async Task<IActionResult> UpdateRole(int roleId, [FromBody] CreateRoleRequest request) {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var role = await context.Roles.FindAsync(roleId);
            if (role == null)
                return NotFound(new { Message = "Role not found." });

            if (await context.Roles.AnyAsync(r => r.Name.ToLower() == request.Name.ToLower() && r.RoleId != roleId))
                ModelState.AddModelError(nameof(request.Name), $"Role '{request.Name.ToLower()}' already exists.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            role.Name = request.Name.Trim();

            context.Roles.Update(role);
            await context.SaveChangesAsync();

            return Ok(role);
        }

        [HttpDelete("{roleId}")]
        public async Task<IActionResult> DeleteRole(int roleId) {
            var role = await context.Roles.FindAsync(roleId);
            if (role == null)
                return NotFound(new { Message = "Role not found." });

            context.Roles.Remove(role);
            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{roleId}/assign")]
        public async Task<IActionResult> AssignRoleToUser(int roleId, [FromBody] AssignRoleRequest request) {
            var role = await context.Roles.FindAsync(roleId);
            if (role == null)
                return NotFound(new { Message = "Role not found." });

            var user = await context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.UserId == request.UserId);

            if (user == null)
                return NotFound(new { Message = "User not found." });

            if (user.UserRoles.Any(ur => ur.RoleId == roleId))
                return BadRequest(new { Message = "User already has this role assigned." });

            user.UserRoles.Add(new UserRole {
                UserId = user.UserId,
                RoleId = roleId
            });

            context.Users.Update(user);
            await context.SaveChangesAsync();
            return Ok(new { Message = "Role assigned to user successfully." });
        }

        [HttpPost("{roleId}/revoke")]
        public async Task<IActionResult> RevokeRoleFromUser(int roleId, [FromBody] AssignRoleRequest request) {
            var role = await context.Roles.FindAsync(roleId);
            if (role == null)
                return NotFound(new { Message = "Role not found." });

            var user = await context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.UserId == request.UserId);

            if (user == null)
                return NotFound(new { Message = "User not found." });

            var userRole = user.UserRoles.FirstOrDefault(ur => ur.RoleId == roleId);
            if (userRole == null)
                return BadRequest(new { Message = "User does not have this role assigned." });

            user.UserRoles.Remove(userRole);

            context.Users.Update(user);
            await context.SaveChangesAsync();

            return Ok(new { Message = "Role revoked from user successfully." });
        }

    }
}
