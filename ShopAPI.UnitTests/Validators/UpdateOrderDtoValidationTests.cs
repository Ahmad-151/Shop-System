using ShopAPI.DTOClasses;
using ShopAPI.Validators;
using Xunit;

namespace ShopAPI.UnitTests.Validators;

public class UpdateOrderDtoValidationTests
{
    private readonly UpdateOrderDtoValidation _validator;

    public UpdateOrderDtoValidationTests()
    {
        _validator = new UpdateOrderDtoValidation();
    }

    private static UpdateOrderDTO CreateValidUpdate()
    {
        return new UpdateOrderDTO
        {
            Status = "Confirmed",
            DeliveredAt = null
        };
    }

    [Fact]
    public void ValidUpdateOrder_ShouldPassValidation()
    {
        var updateOrder = CreateValidUpdate();

        var result = _validator.Validate(updateOrder);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData("Pending")]
    [InlineData("Confirmed")]
    [InlineData("Shipped")]
    [InlineData("Delivered")]
    [InlineData("Cancelled")]
    public void Status_WhenAllowed_ShouldPassValidation(string status)
    {
        var updateOrder = CreateValidUpdate();
        updateOrder.Status = status;

        var result = _validator.Validate(updateOrder);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Status_WhenEmpty_ShouldFailValidation()
    {
        var updateOrder = CreateValidUpdate();
        updateOrder.Status = string.Empty;

        var result = _validator.Validate(updateOrder);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(UpdateOrderDTO.Status) &&
                error.ErrorMessage ==
                    "Status is required");
    }

    [Fact]
    public void Status_WhenInvalid_ShouldFailValidation()
    {
        var updateOrder = CreateValidUpdate();
        updateOrder.Status = "Unknown";

        var result = _validator.Validate(updateOrder);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(UpdateOrderDTO.Status) &&
                error.ErrorMessage ==
                    "Status must be one of: Pending, Confirmed, Shipped, Delivered, Cancelled");
    }

    [Theory]
    [InlineData("pending")]
    [InlineData("CONFIRMED")]
    [InlineData("shipped")]
    [InlineData("Delivered ")]
    public void Status_WhenCaseOrWhitespaceIsDifferent_ShouldFailValidation(
        string status)
    {
        var updateOrder = CreateValidUpdate();
        updateOrder.Status = status;

        var result = _validator.Validate(updateOrder);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(UpdateOrderDTO.Status));
    }

    [Fact]
    public void DeliveredAt_WhenNull_ShouldPassValidation()
    {
        var updateOrder = CreateValidUpdate();
        updateOrder.DeliveredAt = null;

        var result = _validator.Validate(updateOrder);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void DeliveredAt_WhenInThePast_ShouldPassValidation()
    {
        var updateOrder = CreateValidUpdate();
        updateOrder.DeliveredAt = DateTime.UtcNow.AddDays(-1);

        var result = _validator.Validate(updateOrder);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void DeliveredAt_WhenNow_ShouldPassValidation()
    {
        var updateOrder = CreateValidUpdate();
        updateOrder.DeliveredAt = DateTime.UtcNow.AddSeconds(-1);

        var result = _validator.Validate(updateOrder);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void DeliveredAt_WhenInTheFuture_ShouldFailValidation()
    {
        var updateOrder = CreateValidUpdate();
        updateOrder.DeliveredAt = DateTime.UtcNow.AddMinutes(5);

        var result = _validator.Validate(updateOrder);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(UpdateOrderDTO.DeliveredAt) &&
                error.ErrorMessage ==
                    "Date Can Not Be In The Future");
    }

    [Fact]
    public void InvalidStatusAndFutureDeliveredAt_ShouldReturnErrorsForBothProperties()
    {
        var updateOrder = new UpdateOrderDTO
        {
            Status = "Unknown",
            DeliveredAt = DateTime.UtcNow.AddMinutes(5)
        };

        var result = _validator.Validate(updateOrder);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(UpdateOrderDTO.Status));

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName ==
                    nameof(UpdateOrderDTO.DeliveredAt));
    }
}
