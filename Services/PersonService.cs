using ShopAPI.DTOClasses;
using ShopAPI.Repositories;

namespace ShopAPI.Services
{
    public class PersonService : IPersonService
    {
        private readonly IPersonRepository _personRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public PersonService(IPersonRepository personRepository, IRefreshTokenRepository refreshTokenRepository)
        {
            _personRepository = personRepository;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<List<PersonDTO>> GetAllAsync()
        {
            return await _personRepository.GetAllAsync();
        }

        public async Task<PersonDTO?> GetByIdAsync(int id)
        {
            return await _personRepository.GetByIdAsync(id);
        }

        public async Task<bool> UpdateAsync(UpdatePersonDTO updatePersonDTO, int id)
        {
            var current = await _personRepository.GetByIdAsync(id);
            if (current is null) return false;

            if (!string.Equals(current.PhoneNumber, updatePersonDTO.PhoneNumber, StringComparison.Ordinal))
            {
                var owner = await _personRepository.GetByPhoneNumberAsync(updatePersonDTO.PhoneNumber);
                if (owner is not null && owner.Person_ID != id) return false;
            }

            return await _personRepository.UpdateAsync(updatePersonDTO, id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var deleted = await _personRepository.DeleteAsync(id);
            if (deleted)
                await _refreshTokenRepository.RevokeAllForPersonAsync(id);
            return deleted;
        }

        public Task<bool> BannAsync(BannedPersonDTO bannedPersonDTO, int id)
            => BannInternalAsync(bannedPersonDTO, id, null);

        public Task<bool> BannAsync(BannedPersonDTO bannedPersonDTO, int id, int actorId)
            => BannInternalAsync(bannedPersonDTO, id, actorId);

        private async Task<bool> BannInternalAsync(BannedPersonDTO bannedPersonDTO, int id, int? actorId)
        {
            var person = await _personRepository.GetByIdAsync(id);

            if (person is null) return false;
            var banned = actorId.HasValue
                ? await _personRepository.BannAsync(bannedPersonDTO, id, actorId.Value)
                : await _personRepository.BannAsync(bannedPersonDTO, id);

            if (banned)
                await _refreshTokenRepository.RevokeAllForPersonAsync(id);

            return banned;
        }

        public async Task<bool> AddToFavouriteAsync(int discount, int id)
        {
            var person = await _personRepository.GetByIdAsync(id);
            if (person == null) return false;

            return await _personRepository.AddToFavouriteAsync(discount, id);
        }
    }
}
