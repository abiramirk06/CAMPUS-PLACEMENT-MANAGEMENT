using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CampusPlacementManagement.Models;
using Microsoft.IdentityModel.Tokens;

namespace CampusPlacementManagement.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(ApplicationUser user, IEnumerable<string> roles)
        {
            var jwtSettings = _configuration.GetSection("Jwt");

            string key = jwtSettings["Key"]
                ?? throw new InvalidOperationException("JWT Key is missing.");

            string issuer = jwtSettings["Issuer"]
                ?? throw new InvalidOperationException("JWT Issuer is missing.");

            string audience = jwtSettings["Audience"]
                ?? throw new InvalidOperationException("JWT Audience is missing.");

            int expiryMinutes = int.TryParse(
                jwtSettings["ExpiresInMinutes"], out int minutes)
                ? minutes
                : 60;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim("FullName", user.FullName ?? string.Empty)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key));

            var credentials = new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}