using ShopAPI.DTOClasses;
using ShopAPI.Validators;

namespace ShopAPI.UnitTests.Validators;

public sealed class BannedPersonDtoValidationTests
{
    private readonly BannedPersonDtoValidation _validator = new();

    [Fact]
    public void ValidReason_ShouldPass()
    {
        var result = _validator.Validate(new BannedPersonDTO { Reason = "Repeated fraudulent orders" });
        Assert.True(result.IsValid);
    }

    [Fact]
    public void EmptyReason_ShouldFail()
    {
        var result = _validator.Validate(new BannedPersonDTO { Reason = string.Empty });
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Reason is required");
    }

    [Fact]
    public void NullReason_ShouldFail()
    {
        var result = _validator.Validate(new BannedPersonDTO { Reason = null! });
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Reason is required");
    }

    [Fact]
    public void ReasonShorterThanTenCharacters_ShouldFail()
    {
        var result = _validator.Validate(new BannedPersonDTO { Reason = "Too short" });
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Reason must be at least 10 characters long");
    }

    [Fact]
    public void ReasonWithExactlyTenCharacters_ShouldPass()
    {
        var result = _validator.Validate(new BannedPersonDTO { Reason = "1234567890" });
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ReasonWithExactlyTwoHundredCharacters_ShouldPass()
    {
        var result = _validator.Validate(new BannedPersonDTO { Reason = new string('R', 200) });
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ReasonLongerThanTwoHundredCharacters_ShouldFail()
    {
        var result = _validator.Validate(new BannedPersonDTO { Reason = new string('R', 201) });
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Reason must not exceed 200 characters");
    }
}
