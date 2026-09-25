using ShopAPI.DTOClasses;

namespace ShopAPI.Repositories
{

    public interface IPersonRepository
    {
        Task<List<PersonDTO>> GetAllAsync();
        Task<int> RegisterAsync(RegisterPersonDTO registerDto, string passwordHash);
        Task<PersonDTO> GetByIdAsync(int id);
        Task<bool> UpdateAsync(UpdatePersonDTO personDTO, int id);
        Task<bool> DeleteAsync(int id);
        Task<bool> BannAsync(BannedPersonDTO bannedPersonDTO, int id);
        Task<bool> AddToFavouriteAsync(int discount, int id);
        Task<PersonAuthDTO> GetByPhoneNumberAsync(string phoneNumber);
    }
}
