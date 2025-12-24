using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MinimartApi.Authentications;
using MinimartApi.Dtos;
using MinimartApi.Models;
using MinimartApi.Services;
using MinimartApi.Utilities;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MinimartApi.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase {
        private readonly AppDbContext context;
        private readonly IPasswordHasher<User> passwordHasher;
        private readonly IMemoryCache cache;
        private readonly IEmailSender<User> emailSender;
        private readonly JwtHandler jwtHandler;

        private static readonly string DummyPasswordHash;
        private static readonly User DummyUser;

        private const int MaxConfirmAttempts = 5;
        private const int MaxResetAttempts = 5;
        private static readonly TimeSpan ConfirmAttemptWindow = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan ResetCodeExpiry = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan ResetCooldown = TimeSpan.FromMinutes(60);
        private static readonly TimeSpan ResetAttemptWindow = TimeSpan.FromMinutes(15);

        private static string GetConfirmCodeKey(string email) => $"confirm:code:{email}";
        private static string GetConfirmCooldownKey(string email) => $"confirm:cooldown:{email}";
        private static string GetConfirmAttemptKey(string email) => $"confirm:attempts:{email}";
        private static string GetResetCodeKey(string email) => $"reset:code:{email}";
        private static string GetResetCooldownKey(string email) => $"reset:cooldown:{email}";
        private static string GetResetAttemptKey(string email) => $"reset:attempts:{email}";

        private void IncrementAttempts(string key, int current) {
            cache.Set(key, current + 1, ConfirmAttemptWindow);
        }

        static AuthController() {
            DummyUser = new User { Email = "dummy@user", Username = "dummy_user" };
            DummyPasswordHash = new PasswordHasher<User>().HashPassword(
                DummyUser,
                "DummyPassword!123"
            );
        }

        public AuthController(AppDbContext context, IPasswordHasher<User> passwordHasher, JwtHandler jwtHandler, IMemoryCache cache, IEmailSender<User> emailSender) {
            this.context = context;
            this.passwordHasher = passwordHasher;
            this.jwtHandler = jwtHandler;
            this.cache = cache;
            this.emailSender = emailSender;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request) {
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

            user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

            context.Users.Add(user);

            //TODO: add Role.User to user
            var userRole = await context.Roles.SingleOrDefaultAsync(r => r.Name == Role.User);
            if (userRole != null) {
                context.UserRoles.Add(new UserRole {
                    User = user,
                    Role = userRole
                });
            }

            await context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request) {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var email = request.Email.Trim().ToLowerInvariant();

            var user = await context.Users.SingleOrDefaultAsync(u => u.Email.ToLower() == email);

            var verifyResult = passwordHasher.VerifyHashedPassword(
                user ?? DummyUser,
                user?.PasswordHash ?? DummyPasswordHash,
                request.Password
            );

            if (user == null || verifyResult == PasswordVerificationResult.Failed)
                return Unauthorized();

            var accessToken = await jwtHandler.GenerateAccessToken(user);

            var response = new AccessTokenResponse {
                AccessToken = accessToken
            };

            return Ok(response);
        }

        [HttpPost("resend-confirm-email")]
        public async Task<IActionResult> ResendConfirmEmail([FromBody] ResendConfirmEmailRequest request) {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var email = request.Email.Trim().ToLowerInvariant();
            var user = await context.Users.SingleOrDefaultAsync(u => u.Email.ToLower() == email);
            if (user == null || user.IsEmailConfirmed)
                return Ok();

            var code = Util.GenerateSecureCode();

            cache.Set(GetConfirmCodeKey(email), code, ConfirmAttemptWindow);

            var param = new ConfirmEmailRequest {
                Email = user.Email,
                Code = code
            };

            string? confirmationLink = Url.Action(nameof(ConfirmEmail), "Auth", param, Request.Scheme);
            await emailSender.SendConfirmationLinkAsync(user, user.Email, confirmationLink!);

            return Ok();
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request) {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var email = request.Email.Trim().ToLowerInvariant();
            var providedCode = request.Code.Trim();

            var confirmAttemptKey = GetConfirmAttemptKey(email);
            var confirmCodeKey = GetConfirmCodeKey(email);

            var attempts = cache.GetOrCreate(confirmAttemptKey, entry => {
                entry.AbsoluteExpirationRelativeToNow = ConfirmAttemptWindow;
                return 0;
            });

            if (attempts >= MaxConfirmAttempts) {
                return BadRequest();
            }

            if (!cache.TryGetValue<string>(confirmCodeKey, out var expectedCode)) {
                IncrementAttempts(confirmAttemptKey, attempts);
                return BadRequest();
            }

            if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expectedCode),
                Encoding.UTF8.GetBytes(providedCode))) {

                IncrementAttempts(confirmAttemptKey, attempts);
                return BadRequest();
            }

            var user = await context.Users.SingleOrDefaultAsync(u => u.Email.ToLower() == email);
            if (user == null) {
                IncrementAttempts(confirmAttemptKey, attempts);
                return BadRequest();
            }

            user.IsEmailConfirmed = true;

            cache.Remove(confirmCodeKey);
            cache.Remove(confirmAttemptKey);

            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgottPasswordRequest request) {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var email = request.Email.Trim().ToLowerInvariant();

            var user = await context.Users.SingleOrDefaultAsync(u => u.Email.ToLower() == email);
            if (user == null)
                return Ok();

            if (cache.TryGetValue(GetResetCooldownKey(email), out _))
                return Ok();

            var code = Util.GenerateSecureCode();

            cache.Set(GetResetCodeKey(email), code, ResetCodeExpiry);
            cache.Set(GetResetCooldownKey(email), true, ResetCooldown);
            cache.Remove(GetResetAttemptKey(email));

            await emailSender.SendPasswordResetCodeAsync(user, email, code);

            return Ok();
        }


        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request) {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var email = request.Email.Trim().ToLowerInvariant();
            var providedCode = request.Code.Trim();

            var attemptKey = GetResetAttemptKey(email);

            var attempts = cache.GetOrCreate(attemptKey, entry => {
                entry.AbsoluteExpirationRelativeToNow = ResetAttemptWindow;
                return 0;
            });

            if (attempts >= MaxResetAttempts)
                return BadRequest();

            var resetCodeKey = GetResetCodeKey(email);

            if (!cache.TryGetValue<string>(resetCodeKey, out var expectedCode)) {
                IncrementAttempts(attemptKey, attempts);
                return BadRequest();
            }

            if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expectedCode),
                Encoding.UTF8.GetBytes(providedCode))) {
                IncrementAttempts(attemptKey, attempts);
                return BadRequest();
            }

            var user = await context.Users.SingleOrDefaultAsync(u => u.Email.ToLower() == email);
            if (user == null) {
                IncrementAttempts(attemptKey, attempts);
                return BadRequest();
            }

            user.PasswordHash = passwordHasher.HashPassword(user, request.NewPassword);

            cache.Remove(resetCodeKey);
            cache.Remove(attemptKey);

            await context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUserInfo() {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null) {
                return Unauthorized(new { Message = "User ID claim not found." });
            }
            if (!int.TryParse(userIdClaim.Value, out int userId)) {
                return Unauthorized(new { Message = "Invalid User ID claim." });
            }

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
    }
}
