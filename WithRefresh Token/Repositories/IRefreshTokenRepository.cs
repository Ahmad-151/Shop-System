using ShopAPI.DTOClasses.Auth;

namespace ShopAPI.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task<int> AddAsync(
            string tokenHash,
            int personId,
            DateTime expiresAt);

        Task<RefreshTokenRecordDTO> GetByTokenHashAsync(
            string tokenHash);

        Task<int?> RotateAsync(
            string oldTokenHash,
            string newTokenHash,
            DateTime newExpiresAt);

        Task<bool> RevokeAsync(
            string tokenHash,
            string replacedByTokenHash = null);

        Task<bool> RevokeAllForPersonAsync(int personId);
    }
}
