using ShopAPI.DTOClasses;
using ShopAPI.Validators;
using Xunit;

namespace ShopAPI.UnitTests.Validators;

public class CreateProductDtoValidationTests
{
    private readonly ShopAPI.Validators.CreateProductDtoValidation _validator;

    public CreateProductDtoValidationTests()
    {
        _validator = new ShopAPI.Validators.CreateProductDtoValidation();
    }

    private static CreateProductDTO CreateValidProduct()
    {
        return new CreateProductDTO
        {
            ProductName = "Wireless Keyboard",
            Description =
                "A reliable wireless keyboard for daily office and study use.",
            UnitPrice = 45.50m,
            CategoryId = 2
        };
    }

    [Fact]
    public void ValidProduct_ShouldPassValidation()
    {
        // Arrange

        var product = CreateValidProduct();

        // Act

        var result = _validator.Validate(product);

        // Assert

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ProductName_WhenEmpty_ShouldFailValidation()
    {
        // Arrange

        var product = CreateValidProduct();
        product.ProductName = string.Empty;

        // Act

        var result = _validator.Validate(product);

        // Assert

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName == nameof(CreateProductDTO.ProductName) &&
                error.ErrorMessage == "ProductName is required");
    }

    [Fact]
    public void ProductName_WhenShorterThanFiveCharacters_ShouldFailValidation()
    {
        // Arrange

        var product = CreateValidProduct();
        product.ProductName = "TV";

        // Act

        var result = _validator.Validate(product);

        // Assert

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName == nameof(CreateProductDTO.ProductName) &&
                error.ErrorMessage ==
                    "ProductName must be at least 5 characters long");
    }

    [Fact]
    public void ProductName_WhenLongerThanOneHundredFiftyCharacters_ShouldFailValidation()
    {
        // Arrange

        var product = CreateValidProduct();
        product.ProductName = new string('A', 151);

        // Act

        var result = _validator.Validate(product);

        // Assert

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName == nameof(CreateProductDTO.ProductName) &&
                error.ErrorMessage ==
                    "ProductName must not exceed 150 characters");
    }

    [Fact]
    public void Description_WhenEmpty_ShouldFailValidation()
    {
        // Arrange

        var product = CreateValidProduct();
        product.Description = string.Empty;

        // Act

        var result = _validator.Validate(product);

        // Assert

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName == nameof(CreateProductDTO.Description) &&
                error.ErrorMessage == "Description is required");
    }

    [Fact]
    public void Description_WhenShorterThanTenCharacters_ShouldFailValidation()
    {
        // Arrange

        var product = CreateValidProduct();
        product.Description = "Short";

        // Act

        var result = _validator.Validate(product);

        // Assert

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName == nameof(CreateProductDTO.Description) &&
                error.ErrorMessage ==
                    "Description must be at least 10 characters long");
    }

    [Fact]
    public void Description_WhenLongerThanOneThousandCharacters_ShouldFailValidation()
    {
        // Arrange

        var product = CreateValidProduct();
        product.Description = new string('D', 1001);

        // Act

        var result = _validator.Validate(product);

        // Assert

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName == nameof(CreateProductDTO.Description) &&
                error.ErrorMessage ==
                    "Description must not exceed 1000 characters");
    }

    [Fact]
    public void UnitPrice_WhenLessThanOne_ShouldFailValidation()
    {
        // Arrange

        var product = CreateValidProduct();
        product.UnitPrice = 0.99m;

        // Act

        var result = _validator.Validate(product);

        // Assert

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName == nameof(CreateProductDTO.UnitPrice) &&
                error.ErrorMessage ==
                    "UnitPrice must be a positive number.");
    }

    [Fact]
    public void UnitPrice_WhenZero_ShouldFailValidation()
    {
        // Arrange

        var product = CreateValidProduct();
        product.UnitPrice = 0m;

        // Act

        var result = _validator.Validate(product);

        // Assert

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName == nameof(CreateProductDTO.UnitPrice) &&
                error.ErrorMessage ==
                    "UnitPrice must be a positive number.");
    }

    [Fact]
    public void UnitPrice_WhenNegative_ShouldFailValidation()
    {
        // Arrange

        var product = CreateValidProduct();
        product.UnitPrice = -10m;

        // Act

        var result = _validator.Validate(product);

        // Assert

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName == nameof(CreateProductDTO.UnitPrice) &&
                error.ErrorMessage ==
                    "UnitPrice must be a positive number.");
    }

    [Fact]
    public void CategoryId_WhenZero_ShouldFailValidation()
    {
        // Arrange

        var product = CreateValidProduct();
        product.CategoryId = 0;

        // Act

        var result = _validator.Validate(product);

        // Assert

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName == nameof(CreateProductDTO.CategoryId) &&
                error.ErrorMessage ==
                    "Category Id must be a positive number");
    }

    [Fact]
    public void CategoryId_WhenNegative_ShouldFailValidation()
    {
        // Arrange

        var product = CreateValidProduct();
        product.CategoryId = -1;

        // Act

        var result = _validator.Validate(product);

        // Assert

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName == nameof(CreateProductDTO.CategoryId) &&
                error.ErrorMessage ==
                    "Category Id must be a positive number");
    }

    [Fact]
    public void ProductWithMinimumAllowedValues_ShouldPassValidation()
    {
        // Arrange

        var product = new CreateProductDTO
        {
            ProductName = "ABCDE",
            Description = "1234567890",
            UnitPrice = 1m,
            CategoryId = 1
        };

        // Act

        var result = _validator.Validate(product);

        // Assert

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ProductWithMaximumAllowedTextLengths_ShouldPassValidation()
    {
        // Arrange

        var product = new CreateProductDTO
        {
            ProductName = new string('P', 150),
            Description = new string('D', 1000),
            UnitPrice = 100m,
            CategoryId = 1
        };

        // Act

        var result = _validator.Validate(product);

        // Assert

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }



}
