using ShopAPI.DTOClasses;
using ShopAPI.Validators;
using Xunit;

namespace ShopAPI.UnitTests.Validators;

public class OrderItemDtoValidationTests
{
    private readonly OrderItemDtoValidation _validator;

    public OrderItemDtoValidationTests()
    {
        _validator = new OrderItemDtoValidation();
    }

    private static CreateOrderItemsDTO CreateValidItem()
    {
        return new CreateOrderItemsDTO
        {
            ProductId = 1,
            Quantity = 2
        };
    }

    [Fact]
    public void ValidOrderItem_ShouldPassValidation()
    {
        var item = CreateValidItem();

        var result = _validator.Validate(item);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Quantity_WhenZero_ShouldFailValidation()
    {
        var item = CreateValidItem();
        item.Quantity = 0;

        var result = _validator.Validate(item);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(CreateOrderItemsDTO.Quantity) &&
                error.ErrorMessage ==
                    "Quantity must be between 1 and 1000");
    }

    [Fact]
    public void Quantity_WhenNegative_ShouldFailValidation()
    {
        var item = CreateValidItem();
        item.Quantity = -5;

        var result = _validator.Validate(item);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(CreateOrderItemsDTO.Quantity) &&
                error.ErrorMessage ==
                    "Quantity must be between 1 and 1000");
    }

    [Fact]
    public void Quantity_WhenGreaterThanOneThousand_ShouldFailValidation()
    {
        var item = CreateValidItem();
        item.Quantity = 1001;

        var result = _validator.Validate(item);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(CreateOrderItemsDTO.Quantity) &&
                error.ErrorMessage ==
                    "Quantity must be between 1 and 1000");
    }

    [Fact]
    public void Quantity_WhenEqualToMinimum_ShouldPassValidation()
    {
        var item = CreateValidItem();
        item.Quantity = 1;

        var result = _validator.Validate(item);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Quantity_WhenEqualToMaximum_ShouldPassValidation()
    {
        var item = CreateValidItem();
        item.Quantity = 1000;

        var result = _validator.Validate(item);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ProductId_WhenZero_ShouldFailValidation()
    {
        var item = CreateValidItem();
        item.ProductId = 0;

        var result = _validator.Validate(item);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(CreateOrderItemsDTO.ProductId) &&
                error.ErrorMessage ==
                    "ProductId Must Be a Positive Number");
    }

    [Fact]
    public void ProductId_WhenNegative_ShouldFailValidation()
    {
        var item = CreateValidItem();
        item.ProductId = -1;

        var result = _validator.Validate(item);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(CreateOrderItemsDTO.ProductId) &&
                error.ErrorMessage ==
                    "ProductId Must Be a Positive Number");
    }

    [Fact]
    public void ProductId_WhenPositive_ShouldPassValidation()
    {
        var item = CreateValidItem();
        item.ProductId = 999999;

        var result = _validator.Validate(item);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void InvalidProductIdAndQuantity_ShouldReturnErrorsForBothProperties()
    {
        // Arrange

        var item = new CreateOrderItemsDTO
        {
            ProductId = 0,
            Quantity = 0
        };

        // Act

        var result = _validator.Validate(item);

        // Assert

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(CreateOrderItemsDTO.ProductId));

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(CreateOrderItemsDTO.Quantity));
    }

}
