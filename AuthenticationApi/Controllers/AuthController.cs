using AuthenticationApi.Authentications;
using AuthenticationApi.Service;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace AuthenticationApi.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase {
        private readonly UserManager<IdentityUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly IEmailSender<IdentityUser> emailService;

        private readonly JwtHandler jwtHandler;

        private readonly IConfiguration configuration;

        private readonly string USER_ROLE = "User";

        private readonly long TOKEN_EXPIRY_MINUTES;

        public AuthController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<IdentityUser> signInManager,
            JwtHandler jwtHandler,
            IConfiguration configuration,
            IEmailSender<IdentityUser> emailService) {

            this.userManager = userManager;
            this.roleManager = roleManager;
            this.configuration = configuration;
            this.signInManager = signInManager;
            this.jwtHandler = jwtHandler;
            this.emailService = emailService;

            TOKEN_EXPIRY_MINUTES = Convert.ToInt32(this.configuration["Jwt:ExpireInMinutes"]);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request) {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var user = new IdentityUser {
                UserName = request.Email,
                Email = request.Email,
            };

            var result = await userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded) {
                var errors = result.Errors.ToDictionary(
                    e => e.Code,
                    e => new[] { e.Description }
                );

                return ValidationProblem(new ValidationProblemDetails(errors) {
                    Title = "One or more validation errors occurred.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            await userManager.AddToRoleAsync(user, USER_ROLE);

            //var confirmToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
            //const string confirmEmailUrl = "/confirmEmail";
            //var param = new Dictionary<string, string?>() {
            //    { "userId", user.Id },
            //    { "token", confirmToken }
            //};
            //string confirmationUrl = new Uri(QueryHelpers.AddQueryString(confirmEmailUrl, param)).ToString();

            //await emailService.SendConfirmationLinkAsync(user, request.Email, confirmationUrl);

            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request) {
            var user = await userManager.FindByEmailAsync(request.Email);

            if (user == null)
                return Unauthorized("Invalid credentials.");

            var signIn = await signInManager.CheckPasswordSignInAsync(
                user,
                request.Password,
                lockoutOnFailure: false
            );

            if (signIn.RequiresTwoFactor) {
                if (!string.IsNullOrWhiteSpace(request.TwoFactorCode)) {
                    var result2fa = await signInManager.TwoFactorAuthenticatorSignInAsync(
                        request.TwoFactorCode,
                        isPersistent: false,
                        rememberClient: false
                    );

                    if (!result2fa.Succeeded) {
                        return Unauthorized("Invalid 2FA code.");
                    }

                } else if (!string.IsNullOrWhiteSpace(request.TwoFactorRecoveryCode)) {
                    var resultRecovery = await signInManager.TwoFactorRecoveryCodeSignInAsync(
                        request.TwoFactorRecoveryCode
                    );

                    if (!resultRecovery.Succeeded) {
                        return Unauthorized("Invalid recovery code.");
                    }

                } else {
                    return Unauthorized("Two-factor authentication code is required.");
                }

            } else if (!signIn.Succeeded) {
                return Unauthorized("Invalid credentials.");
            }

            var accessToken = await jwtHandler.GenerateAccessToken(user);
            var refreshToken = await jwtHandler.GenerateRefreshToken(user);

            var response = new AccessTokenResponse {
                AccessToken = accessToken,
                ExpiresIn = TOKEN_EXPIRY_MINUTES * 60,
                RefreshToken = refreshToken
            };

            return Ok(response);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(RefreshRequest request) {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                return BadRequest();

            string userId;
            try {
                userId = jwtHandler.ValidateRefreshToken(request.RefreshToken);
            } catch {
                return Unauthorized();
            }

            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
                return Unauthorized();

            var storedToken = await userManager.GetAuthenticationTokenAsync(
                user,
                JwtHandler.LoginProvider,
                JwtHandler.RefreshTokenName
            );

            if (storedToken == null || storedToken != request.RefreshToken)
                return Unauthorized();

            var newAccessToken = await jwtHandler.GenerateAccessToken(user);
            var refreshToken = await jwtHandler.GenerateRefreshToken(user);

            var response = new AccessTokenResponse {
                AccessToken = newAccessToken,
                ExpiresIn = TOKEN_EXPIRY_MINUTES * 60,
                RefreshToken = refreshToken
            };

            return Ok(response);
        }

        [HttpGet("confirmEmail")]
        public async Task<IActionResult> ConfirmEmail(
            [FromQuery] string userId,
            [FromQuery] string token,
            [FromQuery] string? changedEmail = null
        ) {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token)) {
                return BadRequest();
            }

            if (!ModelState.IsValid) {
                return ValidationProblem(ModelState);
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null) {
                return BadRequest();
            }

            IdentityResult result = await userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded) {
                return BadRequest();
            }

            if (!string.IsNullOrWhiteSpace(changedEmail)) {
                user.Email = changedEmail;
                await userManager.UpdateAsync(user);
            }

            return Ok("Email is confirmed");
        }


        [HttpPost("resendConfirmationEmail")]
        public async Task<IActionResult> ResendConfirmationEmail(ResendConfirmationEmailRequest request) {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null) {
                return Ok();
            }

            if (await userManager.IsEmailConfirmedAsync(user)) {
                return Ok();
            }

            var confirmToken = await userManager.GenerateEmailConfirmationTokenAsync(user);

            var param = new Dictionary<string, string?>() {
                { "userId", user.Id },
                { "token", confirmToken }
            };
            string? confirmationUrl = Url.Action(nameof(ConfirmEmail), "Auth", param, Request.Scheme);


            await emailService.SendConfirmationLinkAsync(user, user.Email, confirmationUrl);

            return Ok();
        }

        [HttpPost("validate")]
        public IActionResult ValidateToken([Required] string token) {
            if (string.IsNullOrWhiteSpace(token)) {
                return BadRequest();
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var keyString = configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(keyString)) {
                return StatusCode(StatusCodes.Status500InternalServerError, "Server configuration error");
            }

            var key = Encoding.UTF8.GetBytes(keyString);

            var validationParameters = new TokenValidationParameters {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = !string.IsNullOrEmpty(configuration["Jwt:Issuer"]),
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidateAudience = !string.IsNullOrEmpty(configuration["Jwt:Audience"]),
                ValidAudience = configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            try {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                var jwtToken = validatedToken as JwtSecurityToken;
                DateTime? expires = jwtToken?.ValidTo;

                var claims = principal.Claims
                    .GroupBy(c => c.Type)
                    .ToDictionary(g => g.Key, g => g.Select(c => c.Value).ToArray());

                var response = new {
                    IsValid = true,
                    ExpiresAt = expires,
                    Claims = claims
                };

                return Ok(response);
            } catch (SecurityTokenExpiredException) {
                return Ok(new { IsValid = false, Error = "expired" });
            } catch (Exception) {
                return Unauthorized();
            }
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> MyInfoFromToken() {
            var user = await userManager.GetUserAsync(User);

            if (user == null)
                return Unauthorized();

            var result = new {
                userId = user.Id,
                email = user.Email,
                userName = user.UserName,
                emailConfirmed = user.EmailConfirmed,
                twoFactorEnabled = user.TwoFactorEnabled,
                roles = await userManager.GetRolesAsync(user)
            };

            return Ok(result);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers() {
            var users = await userManager.Users.Select(u => u.Email).ToListAsync();

            return Ok(new { Count = users.Count, Users = users });
        }

    }
}
