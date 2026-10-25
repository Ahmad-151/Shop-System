using ShopAPI.DTOClasses;
using ShopAPI.Repositories;

namespace ShopAPI.Services
{
    public class PersonService : IPersonService
    {
        private readonly IPersonRepository _personRepository;
        public PersonService(IPersonRepository personRepository)
        {
            _personRepository = personRepository;
        }

        public async Task<List<PersonDTO>> GetAllAsync()
        {
            return await _personRepository.GetAllAsync();
        }

        public async Task<PersonDTO> GetByIdAsync(int id)
        {
            return await _personRepository.GetByIdAsync(id);
        }

        public async Task<bool> UpdateAsync(UpdatePersonDTO updatePersonDTO, int id)
        {
            return await _personRepository.UpdateAsync(updatePersonDTO, id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _personRepository.DeleteAsync(id);
        }

        public async Task<bool> BannAsync(BannedPersonDTO bannedPersonDTO, int id)
        {
            var person = await _personRepository.GetByIdAsync(id);
            if (person == null) return false;

            return await _personRepository.BannAsync(bannedPersonDTO, id);
        }

        public async Task<bool> AddToFavouriteAsync(int discount, int id)
        {
            var person = await _personRepository.GetByIdAsync(id);
            if (person == null) return false;

            return await _personRepository.AddToFavouriteAsync(discount, id);
        }
    }
}
