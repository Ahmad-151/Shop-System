using ShopAPI.DTOClasses;

namespace ShopAPI.Services
{
    public interface IAuthService
    {
        Task<string> LoginAsync(LoginDTO loginDto);
        Task<string> RegisterAsync(RegisterPersonDTO registerDto);
    }
}
