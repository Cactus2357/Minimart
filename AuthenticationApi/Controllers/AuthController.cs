using AuthenticationApi.Authentications;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace AuthenticationApi.Controllers {
    [ApiController]
    //[Route("api/[controller]")]
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
        public async Task<IActionResult> Register([FromBody] RegisterRequest request) {
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

            await userManager.ResetAuthenticatorKeyAsync(user);

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
        public async Task<IActionResult> Login([FromBody] LoginRequest request) {
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
            var user = await jwtHandler.ValidateRefreshToken(request.RefreshToken);
            if (user == null)
                return Unauthorized();

            var accessToken = await jwtHandler.GenerateAccessToken(user);
            var refreshToken = await jwtHandler.GenerateRefreshToken(user);

            var response = new AccessTokenResponse {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = TOKEN_EXPIRY_MINUTES * 60,
            };

            return Ok(response);
        }

        [HttpGet("confirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token, string? changedEmail = null) {
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

            string decodedToken = token;
            try {
                var decodedBytes = WebEncoders.Base64UrlDecode(token);
                decodedToken = Encoding.UTF8.GetString(decodedBytes);
            } catch {
                // leave decodedToken as original token
            }

            IdentityResult result = await userManager.ConfirmEmailAsync(user, decodedToken);
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
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(confirmToken));

            var param = new {
                userId = user.Id,
                token = confirmToken
            };
            string? confirmationUrl = Url.Action(nameof(ConfirmEmail), "Auth", param, Request.Scheme);

            if (string.IsNullOrWhiteSpace(confirmationUrl)) {
                var relative = QueryHelpers.AddQueryString("/confirmEmail", new Dictionary<string, string?> {
                    { "userId", user.Id },
                    { "token", encodedToken }
                });
                var baseUri = new Uri($"{Request.Scheme}://{Request.Host.Value}");
                confirmationUrl = new Uri(baseUri, relative).ToString();
            }

            await emailService.SendConfirmationLinkAsync(user, user.Email, confirmationUrl);

            return Ok();
        }

        [HttpPost("forgotPassword")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request) {
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest("Email is required");

            var user = await userManager.FindByEmailAsync(request.Email);

            if (user == null)
                return Ok();

            var token = await userManager.GeneratePasswordResetTokenAsync(user);

            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            //var param = new Dictionary<string, string?>() {
            //    { "email", request.Email },
            //    { "resetCode", encodedToken }
            //};

            //string? callbackUrl = Url.Action(nameof(ResetPassword), "Auth", param, Request.Scheme);

            //await emailService.SendPasswordResetLinkAsync(user, user.Email, callbackUrl);
            await emailService.SendPasswordResetCodeAsync(user, user.Email, encodedToken);

            return Ok();
        }

        [HttpPost("resetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request) {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return Ok();

            var decodedBytes = WebEncoders.Base64UrlDecode(request.ResetCode);
            var resetCode = Encoding.UTF8.GetString(decodedBytes);

            var result = await userManager.ResetPasswordAsync(user, resetCode, request.NewPassword);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await jwtHandler.InvalidateRefreshToken(user);

            return Ok();
        }

        //TODO: fix workflow
        //[Authorize]
        //[HttpPost("manage/2fa")]
        //public async Task<IActionResult> Manage2Fa(TwoFactorRequest request) {
        //    var user = await userManager.GetUserAsync(User);
        //    if (user == null) return NotFound();

        //    var isMachineRemembered = await signInManager.IsTwoFactorClientRememberedAsync(user);
        //    string? sharedKey = null;
        //    IEnumerable<string>? recoveryCodes = null;

        //    if (request.Enable == true) {
        //        if (string.IsNullOrWhiteSpace(request.TwoFactorCode))
        //            return BadRequest("TwoFactorCode is required when enabling 2FA.");

        //        var verificationCode = request.TwoFactorCode.Replace(" ", "").Replace("-", "");

        //        var isTokenValid = await userManager.VerifyTwoFactorTokenAsync(
        //            user,
        //            userManager.Options.Tokens.AuthenticatorTokenProvider,
        //            verificationCode
        //        );

        //        if (!isTokenValid)
        //            return BadRequest("Invalid authenticator code.");

        //        await userManager.SetTwoFactorEnabledAsync(user, true);

        //        var left = await userManager.CountRecoveryCodesAsync(user);
        //        if (left == 0)
        //            recoveryCodes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
        //    }

        //    if (request.Enable == false) {
        //        await userManager.SetTwoFactorEnabledAsync(user, false);
        //    }

        //    if (request.ResetSharedKey) {
        //        await userManager.ResetAuthenticatorKeyAsync(user);

        //        sharedKey = await userManager.GetAuthenticatorKeyAsync(user);
        //    }

        //    if (request.ResetRecoveryCodes) {
        //        recoveryCodes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
        //    }

        //    if (request.ForgetMachine) {
        //        await signInManager.ForgetTwoFactorClientAsync();
        //    }

        //    var finalSharedKey = sharedKey ?? await userManager.GetAuthenticatorKeyAsync(user);
        //    var remainingRecoveryCodes = await userManager.CountRecoveryCodesAsync(user);
        //    var twoFactorEnabled = await userManager.GetTwoFactorEnabledAsync(user);
        //    var finalMachineRemembered = !request.ForgetMachine && isMachineRemembered;

        //    return Ok(new TwoFactorResponse {
        //        SharedKey = finalSharedKey,
        //        RecoveryCodesLeft = remainingRecoveryCodes,
        //        RecoveryCodes = recoveryCodes?.ToArray(),
        //        IsTwoFactorEnabled = twoFactorEnabled,
        //        IsMachineRemembered = finalMachineRemembered
        //    });
        //}

        public class ValidateTokenRequest {
            public required string Token { get; set; }
        }

        [HttpPost("validateAccessToken")]
        public IActionResult ValidateToken([FromBody] ValidateTokenRequest request) {
            if (string.IsNullOrWhiteSpace(request.Token)) {
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
                var principal = tokenHandler.ValidateToken(request.Token, validationParameters, out SecurityToken validatedToken);

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

        [Authorize(Roles = "Admin")]
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers() {
            var users = await userManager.Users.Select(u => u.Email).ToListAsync();

            return Ok(new { Count = users.Count, Users = users });
        }

    }
}
