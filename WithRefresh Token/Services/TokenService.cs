using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ShopAPI.DTOClasses;
using ShopAPI.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ShopAPI.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;

        public TokenService(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
        }

        public string GenerateToken(PersonAuthDTO person)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, person.Person_ID.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new("PersonName", person.PersonName),
                new("PhoneNumber", person.PhoneNumber),
                new(ClaimTypes.Role, person.Role ?? "Customer")
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    _jwtSettings.ExpiryMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes);
        }

        public string HashRefreshToken(string refreshToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);

            var hash = SHA256.HashData(
                Encoding.UTF8.GetBytes(refreshToken));

            return Convert.ToHexString(hash);
        }
    }
}
