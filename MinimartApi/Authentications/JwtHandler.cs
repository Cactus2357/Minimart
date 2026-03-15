using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MinimartApi.Db.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MinimartApi.Authentications
{
    public class JwtHandler
    {
        private readonly IConfiguration configuration;
        private readonly AppDbContext context;

        public JwtHandler(IConfiguration configuration, AppDbContext context)
        {
            this.configuration = configuration;
            this.context = context;

            var jwtKey = configuration["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(jwtKey))
                throw new InvalidOperationException("JWT signing key is not configured. Please set 'Jwt:Key' in appsettings.json.");

            if (jwtKey.Length < 32)
                throw new InvalidOperationException("JWT signing key must be at least 32 characters (256 bits) for HS256.");
        }

        public async Task<string> GenerateAccessToken(User user, IList<Claim>? additionalClaims = null)
        {
            var userRoles = await context.UserRoles
                .Where(ur => ur.UserId == user.UserId)
                .Select(ur => ur.Role.Name)
                .ToListAsync();

            var claims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, user.Username ?? string.Empty),
            };

            claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

            if (additionalClaims != null)
                claims.AddRange(additionalClaims);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? ""));
            var credientials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(configuration["Jwt:ExpireInMinutes"]));

            var token = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: credientials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
