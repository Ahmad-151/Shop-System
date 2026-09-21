using ShopAPI.DTOClasses;
using ShopAPI.Repositories;

namespace ShopAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly IPersonRepository _personRepository;
        private readonly ITokenService _tokenService;

        public AuthService(IPersonRepository personRepository, ITokenService tokenService)
        {
            _personRepository = personRepository;
            _tokenService = tokenService;
        }

        public async Task<string> LoginAsync(LoginDTO loginDto)
        {
            var person = await _personRepository.GetByPhoneNumberAsync(loginDto.PhoneNumber);
            if (person == null || person.PasswordHash == null)
                return null;

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, person.PasswordHash);
            if (!isPasswordValid)
                return null;

            return _tokenService.GenerateToken(person);
        }
        public async Task<string> RegisterAsync(RegisterPersonDTO registerDto)
        {
            var existing = await _personRepository.GetByPhoneNumberAsync(registerDto.PhoneNumber);
            if (existing != null)
                return null;

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);
            int newId = await _personRepository.RegisterAsync(registerDto, passwordHash);

            var newPerson = new PersonAuthDTO
            {
                Person_ID = newId,
                PersonName = registerDto.PersonName,
                PhoneNumber = registerDto.PhoneNumber,
                Role = "Customer"
            };

            return _tokenService.GenerateToken(newPerson);
        }
    }
}
