using ShopAPI.DTOClasses;
using ShopAPI.Validators;
using Xunit;

namespace ShopAPI.UnitTests.Validators;

public class CategoryDtoValidationTests
{
    private readonly CategoryDtoValidation _validator;

    public CategoryDtoValidationTests()
    {
        _validator = new CategoryDtoValidation();
    }

    private static CategoryDTO CreateValidCategory()
    {
        return new CategoryDTO
        {
            CategoryId = 1,
            CategoryName = "Electronics"
        };
    }

    [Fact]
    public void ValidCategory_ShouldPassValidation()
    {
        var category = CreateValidCategory();

        var result = _validator.Validate(category);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void CategoryName_WhenEmpty_ShouldFailValidation()
    {
        var category = CreateValidCategory();
        category.CategoryName = string.Empty;

        var result = _validator.Validate(category);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(CategoryDTO.CategoryName) &&
                error.ErrorMessage ==
                    "Category name is required");
    }

    [Fact]
    public void CategoryName_WhenNull_ShouldFailValidation()
    {
        var category = CreateValidCategory();
        category.CategoryName = null!;

        var result = _validator.Validate(category);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(CategoryDTO.CategoryName) &&
                error.ErrorMessage ==
                    "Category name is required");
    }

    [Fact]
    public void CategoryName_WhenLongerThanOneHundredCharacters_ShouldFailValidation()
    {
        var category = CreateValidCategory();
        category.CategoryName = new string('C', 101);

        var result = _validator.Validate(category);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(CategoryDTO.CategoryName) &&
                error.ErrorMessage ==
                    "Category name must not exceed 100 characters");
    }

    [Fact]
    public void CategoryName_WhenExactlyOneHundredCharacters_ShouldPassValidation()
    {
        var category = CreateValidCategory();
        category.CategoryName = new string('C', 100);

        var result = _validator.Validate(category);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void CategoryName_WhenOneCharacter_ShouldPassValidation()
    {
        var category = CreateValidCategory();
        category.CategoryName = "A";

        var result = _validator.Validate(category);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void CategoryName_WhenContainsSpacesAndLetters_ShouldPassValidation()
    {
        var category = CreateValidCategory();
        category.CategoryName = "Home Appliances";

        var result = _validator.Validate(category);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void CategoryName_WhenContainsArabicCharacters_ShouldPassValidation()
    {
        var category = CreateValidCategory();
        category.CategoryName = "أجهزة إلكترونية";

        var result = _validator.Validate(category);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void CategoryId_ShouldNotAffectValidation()
    {
        var category = new CategoryDTO
        {
            CategoryId = 0,
            CategoryName = "Electronics"
        };

        var result = _validator.Validate(category);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void InvalidCategoryName_ShouldReturnOnlyCategoryNameError()
    {
        var category = new CategoryDTO
        {
            CategoryId = 1,
            CategoryName = string.Empty
        };

        var result = _validator.Validate(category);

        Assert.False(result.IsValid);

        Assert.Single(result.Errors);

        Assert.Equal(
            nameof(CategoryDTO.CategoryName),
            result.Errors[0].PropertyName);

        Assert.Equal(
            "Category name is required",
            result.Errors[0].ErrorMessage);
    }

    [Fact]
    public void CategoryName_WhenMoreThanOneHundredCharacters_ShouldReturnCategoryNameError()
    {
        var category = new CategoryDTO
        {
            CategoryId = 1,
            CategoryName = new string('X', 101)
        };

        var result = _validator.Validate(category);

        Assert.False(result.IsValid);

        Assert.Single(result.Errors);

        Assert.Equal(
            nameof(CategoryDTO.CategoryName),
            result.Errors[0].PropertyName);

        Assert.Equal(
            "Category name must not exceed 100 characters",
            result.Errors[0].ErrorMessage);
    }
}
