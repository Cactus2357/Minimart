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
        //public static readonly string AccessTokenName = "AccessToken";
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
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            };

            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var userClaims = await userManager.GetClaimsAsync(user);
            claims.AddRange(userClaims);



            if (additionalClaims != null)
                claims.AddRange(additionalClaims);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
            var credientials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.AddMinutes(
                Convert.ToDouble(config["Jwt:ExpireInMinutes"])
            );

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
            var tokenId = Guid.NewGuid().ToString("N");
            var refreshExpiryDays = Convert.ToInt32(config["Jwt:RefreshInDays"]);
            var expiryUtc = DateTime.UtcNow.AddDays(refreshExpiryDays).Ticks;

            var raw = $"{user.Id}:{tokenId}:{expiryUtc}";
            var protectedRefresh = refreshProtector.Protect(raw);

            await userManager.SetAuthenticationTokenAsync(
                user,
                LoginProvider,
                RefreshTokenName,
                protectedRefresh
            );

            return protectedRefresh;
        }

        public async Task<IdentityUser?> ValidateRefreshToken(string refreshToken) {
            string payload;
            try {
                payload = refreshProtector.Unprotect(refreshToken);
            } catch {
                return null;
            }

            var parts = payload.Split(':');
            if (parts.Length < 3)
                return null;

            var userId = parts[0];
            var tokenId = parts[1];
            var expiryTicksStr = parts[2];

            if (!long.TryParse(expiryTicksStr, out long expiryTicks))
                return null;

            var expiryUtc = new DateTime(expiryTicks, DateTimeKind.Utc);
            if (expiryUtc < DateTime.UtcNow)
                return null;

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return null;

            var storedToken = await userManager.GetAuthenticationTokenAsync(
                user,
                LoginProvider,
                RefreshTokenName
            );

            if (storedToken != refreshToken)
                return null;

            return user;
        }

        public async Task InvalidateRefreshToken(IdentityUser user) {
            await userManager.RemoveAuthenticationTokenAsync(
                user,
                LoginProvider,
                RefreshTokenName
            );
        }
    }
}
