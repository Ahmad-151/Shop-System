using ShopAPI.DTOClasses;

namespace ShopAPI.Services
{
    public interface ITokenService
    {
        string GenerateToken(PersonAuthDTO person);
        string GenerateRefreshToken();
        string HashRefreshToken(string refreshToken);
    }
}
