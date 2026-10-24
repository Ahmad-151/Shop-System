using ShopAPI.DTOClasses;
using ShopAPI.Validators;
using Xunit;

namespace ShopAPI.UnitTests.Validators;

public class LoginDtoValidationTests
{
    private readonly LoginDTOValidator _validator;

    public LoginDtoValidationTests()
    {
        _validator = new LoginDTOValidator();
    }

    private static LoginDTO CreateValidLogin()
    {
        return new LoginDTO
        {
            PhoneNumber = "0912345678",
            Password = "Password123"
        };
    }

    [Fact]
    public void ValidLogin_ShouldPassValidation()
    {
        var login = CreateValidLogin();

        var result = _validator.Validate(login);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void PhoneNumber_WhenEmpty_ShouldFailValidation()
    {
        var login = CreateValidLogin();
        login.PhoneNumber = string.Empty;

        var result = _validator.Validate(login);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(LoginDTO.PhoneNumber) &&
                error.ErrorMessage ==
                    "Phone number is required");
    }

    [Theory]
    [InlineData("123456789")]
    [InlineData("091234567")]
    [InlineData("0812345678")]
    [InlineData("09123456789")]
    [InlineData("09abcdefgh")]
    [InlineData("+963912345678")]
    [InlineData("09 12345678")]
    public void PhoneNumber_WhenInvalid_ShouldFailValidation(
        string phoneNumber)
    {
        var login = CreateValidLogin();
        login.PhoneNumber = phoneNumber;

        var result = _validator.Validate(login);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(LoginDTO.PhoneNumber) &&
                error.ErrorMessage ==
                    "Phone number must be a valid Syrian number (format: 09xxxxxxxx)");
    }

    [Theory]
    [InlineData("0912345678")]
    [InlineData("0998765432")]
    public void PhoneNumber_WhenValidSyrianNumber_ShouldPassValidation(
        string phoneNumber)
    {
        var login = CreateValidLogin();
        login.PhoneNumber = phoneNumber;

        var result = _validator.Validate(login);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Password_WhenEmpty_ShouldFailValidation()
    {
        var login = CreateValidLogin();
        login.Password = string.Empty;

        var result = _validator.Validate(login);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(LoginDTO.Password) &&
                error.ErrorMessage ==
                    "Password is required");
    }

    [Fact]
    public void Password_WhenShorterThanEightCharacters_ShouldFailValidation()
    {
        var login = CreateValidLogin();
        login.Password = "Pass123";

        var result = _validator.Validate(login);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(LoginDTO.Password) &&
                error.ErrorMessage ==
                    "Password must be at least 8 characters");
    }

    [Fact]
    public void Password_WhenExactlyEightCharacters_ShouldPassValidation()
    {
        var login = CreateValidLogin();
        login.Password = "Pass1234";

        var result = _validator.Validate(login);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Password_WhenLongerThanSeventyTwoBytes_ShouldFailValidation()
    {
        var login = CreateValidLogin();
        login.Password = new string('A', 73);

        var result = _validator.Validate(login);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(LoginDTO.Password) &&
                error.ErrorMessage ==
                    "Password must not exceed 72 bytes");
    }

    [Fact]
    public void Password_WhenExactlySeventyTwoAsciiBytes_ShouldPassValidation()
    {
        var login = CreateValidLogin();
        login.Password = new string('A', 72);

        var result = _validator.Validate(login);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Password_WhenMoreThanSeventyTwoBytesUsingUtf8Characters_ShouldFailValidation()
    {
        var login = CreateValidLogin();
        login.Password = new string('أ', 37);

        var result = _validator.Validate(login);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(LoginDTO.Password) &&
                error.ErrorMessage ==
                    "Password must not exceed 72 bytes");
    }

    [Fact]
    public void MultipleInvalidFields_ShouldReturnErrorsForAllInvalidFields()
    {
        var login = new LoginDTO
        {
            PhoneNumber = "123",
            Password = "123"
        };

        var result = _validator.Validate(login);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(LoginDTO.PhoneNumber));

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(LoginDTO.Password));
    }
}
