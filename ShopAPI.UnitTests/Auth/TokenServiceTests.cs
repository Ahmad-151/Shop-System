using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ShopAPI.DTOClasses;
using ShopAPI.Services;
using ShopAPI.Settings;

namespace ShopAPI.UnitTests.Auth;

public sealed class TokenServiceTests
{
    private static readonly JwtSettings Settings = new()
    {
        Key = "a-strong-test-signing-key-with-at-least-32-bytes",
        Issuer = "ShopAPI",
        Audience = "ShopAPIClients",
        ExpiryMinutes = 30,
        RefreshTokenExpiryDays = 7
    };

    private static TokenService CreateService() => new(Options.Create(Settings));

    [Fact]
    public void GenerateToken_ShouldContainExpectedClaims()
    {
        var token = CreateService().GenerateToken(new PersonAuthDTO
        {
            Person_ID = 7,
            PersonName = "Ahmad",
            Role = "Admin"
        });
        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Equal("7", parsed.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal("Ahmad", parsed.Claims.Single(x => x.Type == "PersonName").Value);
        Assert.Equal("Admin", parsed.Claims.Single(x => x.Type == ClaimTypes.Role).Value);
        Assert.Equal(Settings.Issuer, parsed.Issuer);
        Assert.Contains(Settings.Audience, parsed.Audiences);
    }

    [Fact]
    public void GenerateToken_ShouldBeCryptographicallyValid()
    {
        var token = CreateService().GenerateToken(new PersonAuthDTO { Person_ID = 7, PersonName = "Ahmad", Role = "Customer" });
        var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
        var principal = handler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = Settings.Issuer,
            ValidateAudience = true,
            ValidAudience = Settings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(Settings.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        }, out _);
        Assert.Equal("7", principal.FindFirst("sub")?.Value);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnDifferentNonEmptyValues()
    {
        var service = CreateService();
        var first = service.GenerateRefreshToken();
        var second = service.GenerateRefreshToken();
        Assert.False(string.IsNullOrWhiteSpace(first));
        Assert.NotEqual(first, second);
    }

    [Fact]
    public void HashRefreshToken_ShouldBeDeterministicAndSixtyFourCharacters()
    {
        var service = CreateService();
        var first = service.HashRefreshToken("refresh-token");
        var second = service.HashRefreshToken("refresh-token");
        Assert.Equal(first, second);
        Assert.Equal(64, first.Length);
    }

    [Fact]
    public void HashRefreshToken_ForDifferentValues_ShouldDiffer()
    {
        var service = CreateService();
        Assert.NotEqual(service.HashRefreshToken("one"), service.HashRefreshToken("two"));
    }

    [Fact]
    public void HashRefreshToken_ForMissingValue_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => CreateService().HashRefreshToken(" "));
    }
}
