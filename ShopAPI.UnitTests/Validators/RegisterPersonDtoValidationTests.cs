using ShopAPI.DTOClasses;
using ShopAPI.Validators;
using Xunit;

namespace ShopAPI.UnitTests.Validators;

public class RegisterPersonDtoValidationTests
{
    private readonly RegisterPersonDtoValidation _validator;

    public RegisterPersonDtoValidationTests()
    {
        _validator = new RegisterPersonDtoValidation();
    }

    private static RegisterPersonDTO CreateValidRegistration()
    {
        return new RegisterPersonDTO
        {
            PersonName = "Ahmad Ali",
            PhoneNumber = "0912345678",
            Address = "Damascus, Syria",
            Password = "Password123"
        };
    }

    [Fact]
    public void ValidRegistration_ShouldPassValidation()
    {
        var registration = CreateValidRegistration();

        var result = _validator.Validate(registration);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void PersonName_WhenEmpty_ShouldFailValidation()
    {
        var registration = CreateValidRegistration();
        registration.PersonName = string.Empty;

        var result = _validator.Validate(registration);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(RegisterPersonDTO.PersonName) &&
                error.ErrorMessage ==
                    "Person name is required");
    }

    [Fact]
    public void PersonName_WhenShorterThanThreeCharacters_ShouldFailValidation()
    {
        var registration = CreateValidRegistration();
        registration.PersonName = "A";

        var result = _validator.Validate(registration);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(RegisterPersonDTO.PersonName) &&
                error.ErrorMessage ==
                    "Person name must be at least 3 characters long");
    }

    [Fact]
    public void PersonName_WhenLongerThanOneHundredFiftyCharacters_ShouldFailValidation()
    {
        var registration = CreateValidRegistration();
        registration.PersonName = new string('A', 151);

        var result = _validator.Validate(registration);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(RegisterPersonDTO.PersonName) &&
                error.ErrorMessage ==
                    "Person name must not exceed 150 characters");
    }

    [Fact]
    public void PersonName_WhenExactlyThreeCharacters_ShouldPassValidation()
    {
        var registration = CreateValidRegistration();
        registration.PersonName = "Ali";

        var result = _validator.Validate(registration);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void PersonName_WhenExactlyOneHundredFiftyCharacters_ShouldPassValidation()
    {
        var registration = CreateValidRegistration();
        registration.PersonName = new string('A', 150);

        var result = _validator.Validate(registration);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Address_WhenEmpty_ShouldFailValidation()
    {
        var registration = CreateValidRegistration();
        registration.Address = string.Empty;

        var result = _validator.Validate(registration);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(RegisterPersonDTO.Address) &&
                error.ErrorMessage ==
                    "Address is required");
    }

    [Fact]
    public void Address_WhenLongerThanTwoHundredFiftyCharacters_ShouldFailValidation()
    {
        var registration = CreateValidRegistration();
        registration.Address = new string('A', 251);

        var result = _validator.Validate(registration);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(RegisterPersonDTO.Address) &&
                error.ErrorMessage ==
                    "Address must not exceed 250 characters");
    }

    [Fact]
    public void Address_WhenExactlyTwoHundredFiftyCharacters_ShouldPassValidation()
    {
        var registration = CreateValidRegistration();
        registration.Address = new string('A', 250);

        var result = _validator.Validate(registration);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void PhoneNumber_WhenEmpty_ShouldFailValidation()
    {
        var registration = CreateValidRegistration();
        registration.PhoneNumber = string.Empty;

        var result = _validator.Validate(registration);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(RegisterPersonDTO.PhoneNumber) &&
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
    public void PhoneNumber_WhenInvalid_ShouldFailValidation(string phoneNumber)
    {
        var registration = CreateValidRegistration();
        registration.PhoneNumber = phoneNumber;

        var result = _validator.Validate(registration);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(RegisterPersonDTO.PhoneNumber) &&
                error.ErrorMessage ==
                    "Phone number must be a valid Syrian number (format: 09xxxxxxxx)");
    }

    [Theory]
    [InlineData("0912345678")]
    [InlineData("0998765432")]
    public void PhoneNumber_WhenValidSyrianNumber_ShouldPassValidation(
        string phoneNumber)
    {
        var registration = CreateValidRegistration();
        registration.PhoneNumber = phoneNumber;

        var result = _validator.Validate(registration);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Password_WhenEmpty_ShouldFailValidation()
    {
        var registration = CreateValidRegistration();
        registration.Password = string.Empty;

        var result = _validator.Validate(registration);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(RegisterPersonDTO.Password) &&
                error.ErrorMessage ==
                    "Password is required");
    }

    [Fact]
    public void Password_WhenShorterThanEightCharacters_ShouldFailValidation()
    {
        var registration = CreateValidRegistration();
        registration.Password = "Pass123";

        var result = _validator.Validate(registration);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(RegisterPersonDTO.Password) &&
                error.ErrorMessage ==
                    "Password must be at least 8 characters");
    }

    [Fact]
    public void Password_WhenExactlyEightCharacters_ShouldPassValidation()
    {
        var registration = CreateValidRegistration();
        registration.Password = "Pass1234";

        var result = _validator.Validate(registration);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Password_WhenLongerThanSeventyTwoBytes_ShouldFailValidation()
    {
        var registration = CreateValidRegistration();
        registration.Password = new string('A', 73);

        var result = _validator.Validate(registration);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(RegisterPersonDTO.Password) &&
                error.ErrorMessage ==
                    "Password must not exceed 72 bytes");
    }

    [Fact]
    public void Password_WhenExactlySeventyTwoAsciiBytes_ShouldPassValidation()
    {
        var registration = CreateValidRegistration();
        registration.Password = new string('A', 72);

        var result = _validator.Validate(registration);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Password_WhenMoreThanSeventyTwoBytesUsingUtf8Characters_ShouldFailValidation()
    {
        var registration = CreateValidRegistration();
        registration.Password = new string('أ', 37);

        var result = _validator.Validate(registration);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(RegisterPersonDTO.Password) &&
                error.ErrorMessage ==
                    "Password must not exceed 72 bytes");
    }

    [Fact]
    public void MultipleInvalidFields_ShouldReturnErrorsForAllInvalidFields()
    {
        var registration = new RegisterPersonDTO
        {
            PersonName = "A",
            PhoneNumber = "123",
            Address = string.Empty,
            Password = "123"
        };

        var result = _validator.Validate(registration);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(RegisterPersonDTO.PersonName));

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(RegisterPersonDTO.PhoneNumber));

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(RegisterPersonDTO.Address));

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(RegisterPersonDTO.Password));
    }
}
