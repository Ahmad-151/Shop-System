using ShopAPI.DTOClasses;
using ShopAPI.Validators;
using Xunit;

namespace ShopAPI.UnitTests.Validators;

public class UpdateProductDtoValidationTests
{
    private readonly UpdateProductDtoValidation _validator;

    public UpdateProductDtoValidationTests()
    {
        _validator = new UpdateProductDtoValidation();
    }

    private static UpdateProductDTO CreateValidUpdate()
    {
        return new UpdateProductDTO
        {
            Description =
                "Updated product description for office and study use.",
            UnitPrice = 49.99m
        };
    }

    [Fact]
    public void ValidUpdateProduct_ShouldPassValidation()
    {
        var updateProduct = CreateValidUpdate();

        var result = _validator.Validate(updateProduct);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Description_WhenEmpty_ShouldFailValidation()
    {
        var updateProduct = CreateValidUpdate();
        updateProduct.Description = string.Empty;

        var result = _validator.Validate(updateProduct);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(UpdateProductDTO.Description) &&
                error.ErrorMessage ==
                    "Description is required");
    }

    [Fact]
    public void Description_WhenShorterThanTenCharacters_ShouldFailValidation()
    {
        var updateProduct = CreateValidUpdate();
        updateProduct.Description = "Short";

        var result = _validator.Validate(updateProduct);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(UpdateProductDTO.Description) &&
                error.ErrorMessage ==
                    "Description must be at least 10 characters long");
    }

    [Fact]
    public void Description_WhenExactlyTenCharacters_ShouldPassValidation()
    {
        var updateProduct = CreateValidUpdate();
        updateProduct.Description = "1234567890";

        var result = _validator.Validate(updateProduct);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Description_WhenExactlyOneThousandCharacters_ShouldPassValidation()
    {
        var updateProduct = CreateValidUpdate();
        updateProduct.Description = new string('D', 1000);

        var result = _validator.Validate(updateProduct);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Description_WhenLongerThanOneThousandCharacters_ShouldFailValidation()
    {
        var updateProduct = CreateValidUpdate();
        updateProduct.Description = new string('D', 1001);

        var result = _validator.Validate(updateProduct);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(UpdateProductDTO.Description) &&
                error.ErrorMessage ==
                    "Description must not exceed 1000 characters");
    }

    [Fact]
    public void UnitPrice_WhenZero_ShouldFailValidationAsRequired()
    {
        var updateProduct = CreateValidUpdate();
        updateProduct.UnitPrice = 0m;

        var result = _validator.Validate(updateProduct);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(UpdateProductDTO.UnitPrice) &&
                error.ErrorMessage ==
                    "UnitPrice is required");
    }

    [Fact]
    public void UnitPrice_WhenNegative_ShouldFailValidation()
    {
        var updateProduct = CreateValidUpdate();
        updateProduct.UnitPrice = -10m;

        var result = _validator.Validate(updateProduct);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(UpdateProductDTO.UnitPrice) &&
                error.ErrorMessage ==
                    "UnitPrice must be a positive number .");
    }

    [Fact]
    public void UnitPrice_WhenExactlyOne_ShouldPassValidation()
    {
        var updateProduct = CreateValidUpdate();
        updateProduct.UnitPrice = 1m;

        var result = _validator.Validate(updateProduct);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void UnitPrice_WhenGreaterThanOne_ShouldPassValidation()
    {
        var updateProduct = CreateValidUpdate();
        updateProduct.UnitPrice = 100.50m;

        var result = _validator.Validate(updateProduct);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void DescriptionWithOneHundredOneCharacters_ShouldPassCurrentValidation()
    {
        var updateProduct = CreateValidUpdate();
        updateProduct.Description = new string('D', 101);

        var result = _validator.Validate(updateProduct);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void InvalidDescriptionAndUnitPrice_ShouldReturnErrorsForBothProperties()
    {
        var updateProduct = new UpdateProductDTO
        {
            Description = "Short",
            UnitPrice = -5m
        };

        var result = _validator.Validate(updateProduct);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(UpdateProductDTO.Description));

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(UpdateProductDTO.UnitPrice));
    }
}
