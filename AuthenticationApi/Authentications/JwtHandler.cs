using Azure.Core;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthenticationApi.Authentications {
    public class JwtHandler {
        public static readonly string LoginProvider = "Default";
        public static readonly string AccessTokenName = "AccessToken";
        public static readonly string RefreshTokenName = "RefreshToken";

        private readonly UserManager<IdentityUser> userManager;
        private readonly IConfiguration config;
        private readonly IDataProtector refreshProtector;

        public JwtHandler(IConfiguration config, UserManager<IdentityUser> userManager, IDataProtectionProvider provider) {
            this.config = config;
            this.userManager = userManager;
            this.refreshProtector = provider.CreateProtector("refresh-token");
        }
        public async Task<string> GenerateAccessToken(IdentityUser user, IList<Claim>? additionalClaims = null) {
            var roles = await userManager.GetRolesAsync(user);

            var claims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            };
            //var claims = (await userManager.GetClaimsAsync(user)).ToList();

            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            if (additionalClaims != null) 
                claims.AddRange(additionalClaims);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
            var credientials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(config["Jwt:ExpireInMinutes"]));

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: credientials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> GenerateRefreshToken(IdentityUser user) {
            var refreshToken = Guid.NewGuid().ToString("N");
            var protectedRefresh = refreshProtector.Protect($"{user.Id}:{refreshToken}");

            await userManager.SetAuthenticationTokenAsync(
                user,
                LoginProvider,
                RefreshTokenName,
                protectedRefresh
            );

            return protectedRefresh;
        }

        public string ValidateRefreshToken(string refreshToken) {
            try {
                var payload = refreshProtector.Unprotect(refreshToken);
                var userId = payload.Split(':')[0];

                return userId;
            } catch {
                throw;
            }
        }
    }
}
