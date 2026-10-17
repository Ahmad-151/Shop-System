using Microsoft.Extensions.Options;
using ShopAPI.DTOClasses;
using ShopAPI.DTOClasses.Auth;
using ShopAPI.Repositories;
using ShopAPI.Settings;

namespace ShopAPI.Services
{
    public class AuthService : IAuthService
    {
        private static readonly Lazy<string> DummyHash = new (() => BCrypt.Net.BCrypt.HashPassword("timing-equalizer-not-a-real-password"));

        private readonly IPersonRepository _personRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ITokenService _tokenService;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
                           IPersonRepository personRepository,
                           IRefreshTokenRepository refreshTokenRepository,
                           ITokenService tokenService,
                           IOptions<JwtSettings> jwtSettings,
                           ILogger<AuthService> logger
                           )
        {
            _personRepository = personRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _tokenService = tokenService;
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
        }

        public async Task<TokenResponseDTO> LoginAsync(LoginDTO loginDto)
        {
            var person = await _personRepository.GetByPhoneNumberAsync(loginDto.PhoneNumber);

            if (person is null || !person.IsActive || string.IsNullOrWhiteSpace(person.PasswordHash))
            {
                if (person is null || string.IsNullOrWhiteSpace(person.PasswordHash))
                    BCrypt.Net.BCrypt.Verify(loginDto.Password, DummyHash.Value);
                else
                    BCrypt.Net.BCrypt.Verify(loginDto.Password, person.PasswordHash);

                _logger.LogWarning("Login failed for phone {PhoneNumber}: account is unavailable", MaskPhone(loginDto.PhoneNumber));
                return null;
            }

            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, person.PasswordHash))
            {
                _logger.LogWarning("Login failed for phone {PhoneNumber}: invalid credentials", MaskPhone(loginDto.PhoneNumber));
                return null;
            }

            return await IssueTokenPairAsync(person);
        }

        public async Task<TokenResponseDTO> RegisterAsync(RegisterPersonDTO registerDto)
        {
            var existing = await _personRepository.GetByPhoneNumberAsync(registerDto.PhoneNumber);

            if (existing is not null)
            {
                _logger.LogWarning("Registration rejected because phone {PhoneNumber} already exists", MaskPhone(registerDto.PhoneNumber));
                return null;
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);
            var newId = await _personRepository.RegisterAsync(registerDto, passwordHash);

            var person = new PersonAuthDTO
            {
                Person_ID = newId,
                PersonName = registerDto.PersonName,
                PhoneNumber = registerDto.PhoneNumber,
                Role = "Customer",
                IsActive = true
            };

            return await IssueTokenPairAsync(person);
        }

        public async Task<TokenResponseDTO> RefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return null;

            var oldTokenHash = _tokenService.HashRefreshToken(refreshToken);

            var stored = await _refreshTokenRepository.GetByTokenHashAsync(oldTokenHash);

            if (stored is null)
            {
                _logger.LogWarning("Refresh token rejected because its hash was not found");
                return null;
            }

            if (stored.RevokedAt is not null)
            {
                _logger.LogWarning("Refresh token reuse detected for person {PersonId}; revoking all sessions", stored.Person_ID);
                await _refreshTokenRepository.RevokeAllForPersonAsync(stored.Person_ID);
                return null;
            }

            if (stored.ExpiresAt <= DateTime.UtcNow)
            {
                _logger.LogWarning("Expired refresh token rejected for person {PersonId}", stored.Person_ID);
                return null;
            }

            var person = await _personRepository.GetByIdAsync(stored.Person_ID);

            if (person is null || !person.IsActive)
            return null;

            var personAuth = new PersonAuthDTO
            {
                Person_ID = person.PersonId,
                PersonName = person.PersonName,
                PhoneNumber = person.PhoneNumber,
                Role = person.Role,
                IsActive = person.IsActive
            };

            var accessToken = _tokenService.GenerateToken(personAuth);
            var newRefreshToken = _tokenService.GenerateRefreshToken();
            var newTokenHash = _tokenService.HashRefreshToken(newRefreshToken);
            var newExpiresAt = DateTime.UtcNow.AddDays( _jwtSettings.RefreshTokenExpiryDays);

            var rotatedPersonId = await _refreshTokenRepository.RotateAsync(oldTokenHash,newTokenHash, newExpiresAt);

            if (rotatedPersonId is null)
            {
                await _refreshTokenRepository.RevokeAllForPersonAsync(stored.Person_ID);
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

            var tokenHash = _tokenService .HashRefreshToken(refreshToken);

            await _refreshTokenRepository.RevokeAsync(tokenHash);
        }

        private static string MaskPhone(string? phone) =>
            string.IsNullOrEmpty(phone) || phone.Length < 6
                ? "***"
                : string.Concat(phone.AsSpan(0, 2), new string('*', phone.Length - 5), phone.AsSpan(phone.Length - 3));

        private async Task<TokenResponseDTO> IssueTokenPairAsync(PersonAuthDTO person)
        {
            var accessToken = _tokenService.GenerateToken(person);
            var refreshToken = _tokenService.GenerateRefreshToken();
            var tokenHash = _tokenService.HashRefreshToken(refreshToken);

            var expiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays);

            await _refreshTokenRepository.AddAsync( tokenHash, person.Person_ID,expiresAt);

            return new TokenResponseDTO
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                RefreshTokenExpiresAt = expiresAt
            };
        }
    }
}
