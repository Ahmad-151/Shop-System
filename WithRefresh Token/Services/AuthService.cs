using Microsoft.Extensions.Options;
using ShopAPI.DTOClasses;
using ShopAPI.DTOClasses.Auth;
using ShopAPI.Repositories;
using ShopAPI.Settings;

namespace ShopAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly IPersonRepository _personRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ITokenService _tokenService;
        private readonly JwtSettings _jwtSettings;

        public AuthService(
            IPersonRepository personRepository,
            IRefreshTokenRepository refreshTokenRepository,
            ITokenService tokenService,
            IOptions<JwtSettings> jwtSettings)
        {
            _personRepository = personRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _tokenService = tokenService;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<TokenResponseDTO> LoginAsync(LoginDTO loginDto)
        {
            var person = await _personRepository
                .GetByPhoneNumberAsync(loginDto.PhoneNumber);

            if (person is null || string.IsNullOrWhiteSpace(person.PasswordHash))
                return null;

            if (!BCrypt.Net.BCrypt.Verify(
                    loginDto.Password,
                    person.PasswordHash))
            {
                return null;
            }

            return await IssueTokenPairAsync(person);
        }

        public async Task<TokenResponseDTO> RegisterAsync(
            RegisterPersonDTO registerDto)
        {
            var existing = await _personRepository
                .GetByPhoneNumberAsync(registerDto.PhoneNumber);

            if (existing is not null)
                return null;

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(
                registerDto.Password);
            var newId = await _personRepository.RegisterAsync(
                registerDto,
                passwordHash);

            var person = new PersonAuthDTO
            {
                Person_ID = newId,
                PersonName = registerDto.PersonName,
                PhoneNumber = registerDto.PhoneNumber,
                Role = "Customer"
            };

            return await IssueTokenPairAsync(person);
        }

        public async Task<TokenResponseDTO> RefreshTokenAsync(
            string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return null;

            var oldTokenHash = _tokenService
                .HashRefreshToken(refreshToken);

            var stored = await _refreshTokenRepository
                .GetByTokenHashAsync(oldTokenHash);

            if (stored is null)
                return null;

            if (stored.RevokedAt is not null)
            {
                // Reuse of a rotated token invalidates the token family.
                await _refreshTokenRepository
                    .RevokeAllForPersonAsync(stored.Person_ID);
                return null;
            }

            if (stored.ExpiresAt <= DateTime.UtcNow)
                return null;

            var person = await _personRepository
                .GetByIdAsync(stored.Person_ID);

            if (person is null)
                return null;

            var personAuth = new PersonAuthDTO
            {
                Person_ID = person.PersonId,
                PersonName = person.PersonName,
                PhoneNumber = person.PhoneNumber,
                Role = person.Role
            };

            var accessToken = _tokenService.GenerateToken(personAuth);
            var newRefreshToken = _tokenService.GenerateRefreshToken();
            var newTokenHash = _tokenService
                .HashRefreshToken(newRefreshToken);
            var newExpiresAt = DateTime.UtcNow.AddDays(
                _jwtSettings.RefreshTokenExpiryDays);

            var rotatedPersonId = await _refreshTokenRepository.RotateAsync(
                oldTokenHash,
                newTokenHash,
                newExpiresAt);

            if (rotatedPersonId is null)
            {
                // A concurrent request may have rotated the same token first.
                await _refreshTokenRepository
                    .RevokeAllForPersonAsync(stored.Person_ID);
                return null;
            }

            return new TokenResponseDTO
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken,
                RefreshTokenExpiresAt = newExpiresAt
            };
        }

        public async Task LogoutAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return;

            var tokenHash = _tokenService
                .HashRefreshToken(refreshToken);

            await _refreshTokenRepository.RevokeAsync(tokenHash);
        }

        private async Task<TokenResponseDTO> IssueTokenPairAsync(
            PersonAuthDTO person)
        {
            var accessToken = _tokenService.GenerateToken(person);
            var refreshToken = _tokenService.GenerateRefreshToken();
            var tokenHash = _tokenService
                .HashRefreshToken(refreshToken);
            var expiresAt = DateTime.UtcNow.AddDays(
                _jwtSettings.RefreshTokenExpiryDays);

            await _refreshTokenRepository.AddAsync(
                tokenHash,
                person.Person_ID,
                expiresAt);

            return new TokenResponseDTO
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                RefreshTokenExpiresAt = expiresAt
            };
        }
    }
}
