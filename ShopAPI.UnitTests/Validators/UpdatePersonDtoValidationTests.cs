using ShopAPI.DTOClasses;
using ShopAPI.Validators;

namespace ShopAPI.UnitTests.Validators;

public sealed class UpdatePersonDtoValidationTests
{
    private readonly UpdatePersonDtoValidation _validator = new();

    private static UpdatePersonDTO Valid() => new()
    {
        PhoneNumber = "0912345678",
        Address = "Damascus, Syria"
    };

    [Fact]
    public void ValidUpdate_ShouldPass()
    {
        Assert.True(_validator.Validate(Valid()).IsValid);
    }

    [Fact]
    public void EmptyAddress_ShouldFail()
    {
        var dto = Valid();
        dto.Address = string.Empty;
        var result = _validator.Validate(dto);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Address is required");
    }

    [Fact]
    public void AddressLongerThanTwoHundredFiftyCharacters_ShouldFail()
    {
        var dto = Valid();
        dto.Address = new string('A', 251);
        var result = _validator.Validate(dto);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdatePersonDTO.Address));
    }

    [Fact]
    public void InvalidPhoneNumber_ShouldFail()
    {
        var dto = Valid();
        dto.PhoneNumber = "123";
        var result = _validator.Validate(dto);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("valid Syrian number"));
    }

    [Fact]
    public void EmptyPhoneNumber_ShouldFail()
    {
        var dto = Valid();
        dto.PhoneNumber = string.Empty;
        var result = _validator.Validate(dto);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Phone number is required");
    }

    [Fact]
    public void InvalidAddressAndPhone_ShouldReturnBothErrors()
    {
        var result = _validator.Validate(new UpdatePersonDTO
        {
            Address = string.Empty,
            PhoneNumber = "123"
        });
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdatePersonDTO.Address));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdatePersonDTO.PhoneNumber));
    }
}
