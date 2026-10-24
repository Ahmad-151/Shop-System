using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using ShopAPI.DTOClasses;
using ShopAPI.DTOClasses.Auth;
using ShopAPI.Repositories;
using ShopAPI.Services;
using ShopAPI.Settings;

namespace ShopAPI.UnitTests.Auth;

public class AuthServiceTests
{
    [Fact]
    public async Task Login_WithWrongPassword_ShouldReturnNull()
    {
        // Arrange

        var loginDto = new LoginDTO
        {
            PhoneNumber = "0912345678",
            Password = "WrongPassword123!"

        };

        var storedPasswordHash =
            BCrypt.Net.BCrypt.HashPassword("CorrectPassword123!");

        var person = new PersonAuthDTO
        {
            Person_ID = 1,
            PersonName = "Test Customer",
            PhoneNumber = loginDto.PhoneNumber,
            PasswordHash = storedPasswordHash,
            Role = "Customer",
            IsActive = true
        };

        var personRepository = new Mock<IPersonRepository>();

        // IF Someone Call GetByPhone Method With this Phone Retrun this persons data 
        personRepository.Setup(repository => repository.GetByPhoneNumberAsync(loginDto.PhoneNumber)).ReturnsAsync(person);

        var refreshTokenRepository = new Mock<IRefreshTokenRepository>();

        var tokenService = new Mock<ITokenService>();

        var jwtSettings = Options.Create(new JwtSettings());

        var authService = new AuthService(personRepository.Object, refreshTokenRepository.Object,
                                            tokenService.Object,jwtSettings,NullLogger<AuthService>.Instance);

        // Act

        var result = await authService.LoginAsync(loginDto);

        // Assert

        Assert.Null(result);

        tokenService.Verify( service => service.GenerateToken(It.IsAny<PersonAuthDTO>()),Times.Never);
    }

    [Fact]
    public async Task Login_WithInactiveAccount_ShouldReturnNull()
    {
        // Arrange

        var loginDto = new LoginDTO
        {
            PhoneNumber = "0912345678",
            Password = "CorrectPassword123!"
        };

        var storedPasswordHash =
            BCrypt.Net.BCrypt.HashPassword(loginDto.Password);

        var inactivePerson = new PersonAuthDTO
        {
            Person_ID = 2,
            PersonName = "Inactive Customer",
            PhoneNumber = loginDto.PhoneNumber,
            PasswordHash = storedPasswordHash,
            Role = "Customer",
            IsActive = false     // Is Not Active
        };

        var personRepository = new Mock<IPersonRepository>();

        personRepository.Setup(repository =>repository.GetByPhoneNumberAsync(loginDto.PhoneNumber)).ReturnsAsync(inactivePerson);

        var refreshTokenRepository = new Mock<IRefreshTokenRepository>();

        var tokenService = new Mock<ITokenService>();

        var jwtSettings = Options.Create(new JwtSettings());

        var authService = new AuthService(personRepository.Object,refreshTokenRepository.Object,tokenService.Object,
                                               jwtSettings,NullLogger<AuthService>.Instance);

        // Act

        var result = await authService.LoginAsync(loginDto);

        // Assert

        Assert.Null(result);

        tokenService.Verify(service => service.GenerateToken(It.IsAny<PersonAuthDTO>()),Times.Never);

        refreshTokenRepository.Verify(repository => repository.AddAsync
                                     (It.IsAny<string>(),It.IsAny<int>(), It.IsAny<DateTime>()),Times.Never);

    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnTokens()
    {
        // Arrange

        var loginDto = new LoginDTO
        {
            PhoneNumber = "0912345678",
            Password = "CorrectPassword123!"
        };

        var storedPasswordHash = BCrypt.Net.BCrypt.HashPassword(loginDto.Password);

        var activePerson = new PersonAuthDTO
        {
            Person_ID = 3,
            PersonName = "Active Customer",
            PhoneNumber = loginDto.PhoneNumber,
            PasswordHash = storedPasswordHash,
            Role = "Customer",
            IsActive = true
        };

        var personRepository = new Mock<IPersonRepository>();

        personRepository.Setup(repository => repository.GetByPhoneNumberAsync(loginDto.PhoneNumber)).ReturnsAsync(activePerson);

        var refreshTokenRepository = new Mock<IRefreshTokenRepository>();

        refreshTokenRepository.Setup(repository => repository.AddAsync
                                    (It.IsAny<string>(),activePerson.Person_ID,It.IsAny<DateTime>())).ReturnsAsync(1);

        var tokenService = new Mock<ITokenService>();

        tokenService.Setup(service => service.GenerateToken(activePerson)) .Returns("fake-access-token");

        tokenService.Setup(service => service.GenerateRefreshToken()).Returns("fake-refresh-token");

        tokenService.Setup(service => service.HashRefreshToken("fake-refresh-token")).Returns("fake-refresh-token-hash");

        var jwtSettings = Options.Create(new JwtSettings{RefreshTokenExpiryDays = 7 });

        var authService = new AuthService(personRepository.Object, refreshTokenRepository.Object,tokenService.Object,
                                            jwtSettings,NullLogger<AuthService>.Instance);

        // Act

        var result = await authService.LoginAsync(loginDto);

        // Assert

        Assert.NotNull(result);

        Assert.Equal("fake-access-token",result!.AccessToken);

        Assert.Equal("fake-refresh-token",result.RefreshToken);

        Assert.True(result.RefreshTokenExpiresAt > DateTime.UtcNow);

        tokenService.Verify(service => service.GenerateToken(activePerson),Times.Once);

        tokenService.Verify(service => service.GenerateRefreshToken(),Times.Once);

        tokenService.Verify(service => service.HashRefreshToken("fake-refresh-token"),Times.Once);

        refreshTokenRepository.Verify(repository => repository.AddAsync
        ("fake-refresh-token-hash",activePerson.Person_ID,It.Is<DateTime>
        (expiration => expiration > DateTime.UtcNow.AddDays(6))),Times.Once);
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ShouldRotateTokens()
    {
        // Arrange

        var oldRefreshToken = "old-refresh-token";
        var oldRefreshTokenHash = "old-refresh-token-hash";

        var newRefreshToken = "new-refresh-token";
        var newRefreshTokenHash = "new-refresh-token-hash";

        var personId = 10;

        var activePerson = new PersonDTO
        {
            PersonId = personId,
            PersonName = "Active Customer",
            PhoneNumber = "0912345678",
            Address = "Test Address",
            Role = "Customer",
            IsActive = true
        };

        var storedRefreshToken = new RefreshTokenRecordDTO
        {
            Token_ID = 50,
            Person_ID = personId,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            RevokedAt = null
        };

        // Person Repository Mock Setup
        var personRepository =new Mock<IPersonRepository>();

        personRepository.Setup(repository => repository.GetByIdAsync(personId)).ReturnsAsync(activePerson);

        // Refresh Token Repository Mock Setup
        var refreshTokenRepository = new Mock<IRefreshTokenRepository>();

        refreshTokenRepository.Setup(repository => repository.GetByTokenHashAsync(oldRefreshTokenHash)).ReturnsAsync(storedRefreshToken);

        refreshTokenRepository.Setup(repository => repository.RotateAsync(oldRefreshTokenHash,newRefreshTokenHash,
                                   It.IsAny<DateTime>())).ReturnsAsync(personId);

        // Token Service Mock Setup
        var tokenService =new Mock<ITokenService>();

        tokenService.Setup(service =>service.HashRefreshToken(oldRefreshToken)).Returns(oldRefreshTokenHash);

        tokenService.Setup(service =>service.GenerateToken(It.IsAny<PersonAuthDTO>())).Returns("new-access-token");

        tokenService.Setup(service =>service.GenerateRefreshToken()).Returns(newRefreshToken);

        tokenService.Setup(service =>service.HashRefreshToken(newRefreshToken)).Returns(newRefreshTokenHash);

        var jwtSettings = Options.Create(new JwtSettings{RefreshTokenExpiryDays = 7});

        var authService = new AuthService(personRepository.Object,refreshTokenRepository.Object,tokenService.Object,
                                          jwtSettings,NullLogger<AuthService>.Instance
        );

        // Act

        var result = await authService.RefreshTokenAsync(oldRefreshToken);

        // Assert

        Assert.NotNull(result);

        Assert.Equal("new-access-token",result!.AccessToken);

        Assert.Equal(newRefreshToken,result.RefreshToken);

        Assert.True(result.RefreshTokenExpiresAt > DateTime.UtcNow);

        tokenService.Verify(service =>service.HashRefreshToken(oldRefreshToken),Times.Once);

        tokenService.Verify(service =>service.GenerateToken
        (It.Is<PersonAuthDTO>
        (person => person.Person_ID == personId &&person.PersonName == activePerson.PersonName &&
                   person.Role == activePerson.Role &&person.IsActive)),Times.Once);

        tokenService.Verify(service =>service.GenerateRefreshToken(),Times.Once);

        tokenService.Verify(service =>service.HashRefreshToken(newRefreshToken),Times.Once);

        refreshTokenRepository.Verify(repository =>repository.GetByTokenHashAsync(oldRefreshTokenHash),Times.Once);

        refreshTokenRepository.Verify(repository =>repository.RotateAsync
        (oldRefreshTokenHash,newRefreshTokenHash,It.Is<DateTime>
        (expiration => expiration > DateTime.UtcNow.AddDays(6))),Times.Once);

        refreshTokenRepository.Verify(repository =>repository.RevokeAllForPersonAsync(It.IsAny<int>()),Times.Never);
    }

    [Fact]
    public async Task RefreshToken_WithExpiredToken_ShouldReturnNull()
    {
        // Arrange

        var oldRefreshToken = "expired-refresh-token";
        var oldRefreshTokenHash = "expired-refresh-token-hash";

        var personId = 20;

        var expiredRefreshToken = new RefreshTokenRecordDTO
        {
            Token_ID = 100,
            Person_ID = personId,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            ExpiresAt = DateTime.UtcNow.AddMinutes(-5),
            RevokedAt = null
        };

        var personRepository =new Mock<IPersonRepository>();

        var refreshTokenRepository =new Mock<IRefreshTokenRepository>();

        refreshTokenRepository.Setup(repository =>repository.GetByTokenHashAsync(oldRefreshTokenHash)).ReturnsAsync(expiredRefreshToken);

        var tokenService =new Mock<ITokenService>();

        tokenService.Setup(service =>service.HashRefreshToken(oldRefreshToken)).Returns(oldRefreshTokenHash);

        var jwtSettings = Options.Create(new JwtSettings{RefreshTokenExpiryDays = 7});

        var authService = new AuthService(personRepository.Object,refreshTokenRepository.Object,tokenService.Object,
                                           jwtSettings,NullLogger<AuthService>.Instance);

        // Act

        var result =await authService.RefreshTokenAsync(oldRefreshToken);

        // Assert

        Assert.Null(result);

        tokenService.Verify(service =>service.HashRefreshToken(oldRefreshToken),Times.Once);

        refreshTokenRepository.Verify(repository =>repository.GetByTokenHashAsync(oldRefreshTokenHash),Times.Once);

        personRepository.Verify(repository =>repository.GetByIdAsync(personId),Times.Never);

        tokenService.Verify(service =>service.GenerateToken(It.IsAny<PersonAuthDTO>()),Times.Never);

        tokenService.Verify(service =>service.GenerateRefreshToken(),Times.Never);

        refreshTokenRepository.Verify(repository =>repository.RotateAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<DateTime>()),
                                       Times.Never);

        refreshTokenRepository.Verify(repository =>repository.RevokeAllForPersonAsync(It.IsAny<int>()),Times.Never);
    }

    [Fact]
    public async Task RefreshToken_WithRevokedToken_ShouldRevokeAllSessions()
    {
        // Arrange

        var revokedRefreshToken = "revoked-refresh-token";
        var revokedRefreshTokenHash = "revoked-refresh-token-hash";

        var personId = 30;

        var revokedToken = new RefreshTokenRecordDTO
        {
            Token_ID = 200,
            Person_ID = personId,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            ExpiresAt = DateTime.UtcNow.AddDays(5),
            RevokedAt = DateTime.UtcNow.AddHours(-1) // Cencelled 
        };

        var personRepository =new Mock<IPersonRepository>();

        var refreshTokenRepository =new Mock<IRefreshTokenRepository>();

        refreshTokenRepository.Setup(repository =>repository.GetByTokenHashAsync(revokedRefreshTokenHash)).ReturnsAsync(revokedToken);

        refreshTokenRepository.Setup(repository =>repository.RevokeAllForPersonAsync(personId)).ReturnsAsync(true);

        var tokenService =new Mock<ITokenService>();

        tokenService.Setup(service =>service.HashRefreshToken(revokedRefreshToken)).Returns(revokedRefreshTokenHash);

        var jwtSettings = Options.Create(new JwtSettings{RefreshTokenExpiryDays = 7});

        var authService = new AuthService(personRepository.Object,refreshTokenRepository.Object,tokenService.Object,
                                            jwtSettings,NullLogger<AuthService>.Instance);

        // Act

        var result =await authService.RefreshTokenAsync(revokedRefreshToken);

        // Assert

        Assert.Null(result);

        tokenService.Verify(service =>service.HashRefreshToken(revokedRefreshToken),Times.Once);

        refreshTokenRepository.Verify(repository =>repository.GetByTokenHashAsync(revokedRefreshTokenHash),Times.Once);

        refreshTokenRepository.Verify(repository =>repository.RevokeAllForPersonAsync(personId),Times.Once);

        personRepository.Verify(repository =>repository.GetByIdAsync(personId),Times.Never);

        tokenService.Verify(service =>service.GenerateToken(It.IsAny<PersonAuthDTO>()),Times.Never);

        tokenService.Verify(service =>service.GenerateRefreshToken(),Times.Never);

        refreshTokenRepository.Verify(repository =>repository.RotateAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<DateTime>()),
                                     Times.Never);
    }

    [Fact]
    public async Task RefreshToken_WithInactiveAccount_ShouldReturnNull()
    {
        // Arrange

        var oldRefreshToken = "inactive-account-refresh-token";
        var oldRefreshTokenHash = "inactive-account-refresh-token-hash";

        var personId = 40;

        var inactivePerson = new PersonDTO
        {
            PersonId = personId,
            PersonName = "Inactive Customer",
            PhoneNumber = "0912345678",
            Address = "Test Address",
            Role = "Customer",
            IsActive = false
        };

        var storedRefreshToken = new RefreshTokenRecordDTO
        {
            Token_ID = 300,
            Person_ID = personId,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            RevokedAt = null
        };

        var personRepository =new Mock<IPersonRepository>();

        personRepository.Setup(repository =>repository.GetByIdAsync(personId)).ReturnsAsync(inactivePerson);

        var refreshTokenRepository =new Mock<IRefreshTokenRepository>();

        refreshTokenRepository.Setup(repository =>repository.GetByTokenHashAsync(oldRefreshTokenHash)).ReturnsAsync(storedRefreshToken);

        var tokenService =new Mock<ITokenService>();

        tokenService.Setup(service =>service.HashRefreshToken(oldRefreshToken)).Returns(oldRefreshTokenHash);

        var jwtSettings = Options.Create(new JwtSettings{RefreshTokenExpiryDays = 7});

        var authService = new AuthService(personRepository.Object,refreshTokenRepository.Object,tokenService.Object,
                                             jwtSettings,NullLogger<AuthService>.Instance);

        // Act

        var result =await authService.RefreshTokenAsync(oldRefreshToken);

        // Assert

        Assert.Null(result);

        tokenService.Verify(service =>service.HashRefreshToken(oldRefreshToken),Times.Once);

        refreshTokenRepository.Verify(repository =>repository.GetByTokenHashAsync(oldRefreshTokenHash),Times.Once);

        personRepository.Verify(repository =>repository.GetByIdAsync(personId),Times.Once);

        tokenService.Verify(service =>service.GenerateToken(It.IsAny<PersonAuthDTO>()),Times.Never);

        tokenService.Verify(service =>service.GenerateRefreshToken(),Times.Never);

        tokenService.Verify(service =>service.HashRefreshToken(oldRefreshToken),Times.Once);

        refreshTokenRepository.Verify(repository =>repository.RotateAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<DateTime>()),
                                     Times.Never);

        refreshTokenRepository.Verify(repository =>repository.RevokeAllForPersonAsync(It.IsAny<int>()),Times.Never);
    }

    [Fact]
    public async Task RefreshToken_WithMissingToken_ShouldReturnNull()
    {
        // Arrange

        var personRepository =new Mock<IPersonRepository>();

        var refreshTokenRepository =new Mock<IRefreshTokenRepository>();

        var tokenService =new Mock<ITokenService>();

        var jwtSettings = Options.Create(new JwtSettings{RefreshTokenExpiryDays = 7});

        var authService = new AuthService(personRepository.Object,refreshTokenRepository.Object,tokenService.Object,
                                           jwtSettings,NullLogger<AuthService>.Instance);

        // Act

        var result =await authService.RefreshTokenAsync(null!);

        // Assert

        Assert.Null(result);

        tokenService.Verify(service =>service.HashRefreshToken(It.IsAny<string>()),Times.Never);

        refreshTokenRepository.Verify(repository =>repository.GetByTokenHashAsync(It.IsAny<string>()),Times.Never);

        personRepository.Verify(repository =>repository.GetByIdAsync(It.IsAny<int>()),Times.Never);

        tokenService.Verify(service =>service.GenerateToken(It.IsAny<PersonAuthDTO>()),Times.Never);

        tokenService.Verify(service =>service.GenerateRefreshToken(),Times.Never);

        refreshTokenRepository.Verify(repository =>repository.RotateAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<DateTime>()),
                                           Times.Never);

        refreshTokenRepository.Verify(repository =>repository.RevokeAllForPersonAsync(It.IsAny<int>()),Times.Never);
    }


}
