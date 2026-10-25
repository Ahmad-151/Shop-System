using ShopAPI.DTOClasses;
using ShopAPI.DTOClasses.Auth;

namespace ShopAPI.Services
{
    public interface IAuthService
    {
        Task<TokenResponseDTO> LoginAsync(LoginDTO loginDto);
        Task<TokenResponseDTO> RegisterAsync(RegisterPersonDTO registerDto);
        Task<TokenResponseDTO> RefreshTokenAsync(string refreshToken);
        Task LogoutAsync(string refreshToken);

    }
}