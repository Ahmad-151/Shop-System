using ShopAPI.DTOClasses;

namespace ShopAPI.Services
{
    public interface IPersonService
    {
        Task<List<PersonDTO>> GetAllAsync();
        Task<PersonDTO> GetByIdAsync(int id);
        Task<bool> UpdateAsync(UpdatePersonDTO personDTO, int id);
        Task<bool> DeleteAsync(int id);
        Task<bool> BannAsync(BannedPersonDTO bannedPersonDTO, int id);
        Task<bool> AddToFavouriteAsync(int discount, int id);
    }
}
