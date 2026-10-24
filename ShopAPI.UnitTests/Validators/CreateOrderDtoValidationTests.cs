using ShopAPI.DTOClasses;
using ShopAPI.Validators;
using Xunit;

namespace ShopAPI.UnitTests.Validators;

public class CreateOrderDtoValidationTests
{
    private readonly CreateOrderDtoValidation _validator;

    public CreateOrderDtoValidationTests()
    {
        _validator = new CreateOrderDtoValidation();
    }

    private static CreateOrderDTO CreateValidOrder()
    {
        return new CreateOrderDTO
        {
            Items = new List<CreateOrderItemsDTO>
            {
                new CreateOrderItemsDTO
                {
                    ProductId = 1,
                    Quantity = 2
                }
            }
        };
    }

    [Fact]
    public void ValidOrder_ShouldPassValidation()
    {
        var order = CreateValidOrder();

        var result = _validator.Validate(order);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void OrderWithMultipleValidItems_ShouldPassValidation()
    {
        var order = new CreateOrderDTO
        {
            Items = new List<CreateOrderItemsDTO>
            {
                new CreateOrderItemsDTO
                {
                    ProductId = 1,
                    Quantity = 2
                },
                new CreateOrderItemsDTO
                {
                    ProductId = 2,
                    Quantity = 5
                },
                new CreateOrderItemsDTO
                {
                    ProductId = 3,
                    Quantity = 10
                }
            }
        };

        var result = _validator.Validate(order);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Items_WhenNull_ShouldFailValidation()
    {
        var order = new CreateOrderDTO
        {
            Items = null!
        };

        var result = _validator.Validate(order);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName == nameof(CreateOrderDTO.Items) &&
                error.ErrorMessage == "Items are required");
    }

    [Fact]
    public void Items_WhenEmpty_ShouldFailValidation()
    {
        var order = new CreateOrderDTO
        {
            Items = new List<CreateOrderItemsDTO>()
        };

        var result = _validator.Validate(order);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName == nameof(CreateOrderDTO.Items) &&
                error.ErrorMessage ==
                    "An order must contain at least one item");
    }

    [Fact]
    public void Items_WhenMoreThanFifty_ShouldFailValidation()
    {
        var items = Enumerable
            .Range(1, 51)
            .Select(productId =>
                new CreateOrderItemsDTO
                {
                    ProductId = productId,
                    Quantity = 1
                })
            .ToList();

        var order = new CreateOrderDTO
        {
            Items = items
        };

        var result = _validator.Validate(order);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName == nameof(CreateOrderDTO.Items) &&
                error.ErrorMessage ==
                    "An order cannot contain more than 50 items");
    }

    [Fact]
    public void Items_WhenSameProductAppearsMoreThanOnce_ShouldFailValidation()
    {
        var order = new CreateOrderDTO
        {
            Items = new List<CreateOrderItemsDTO>
            {
                new CreateOrderItemsDTO
                {
                    ProductId = 1,
                    Quantity = 2
                },
                new CreateOrderItemsDTO
                {
                    ProductId = 1,
                    Quantity = 3
                }
            }
        };

        var result = _validator.Validate(order);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName == nameof(CreateOrderDTO.Items) &&
                error.ErrorMessage ==
                    "Each product may appear only once per order; increase its quantity instead");
    }

    [Fact]
    public void ItemQuantity_WhenZero_ShouldFailValidation()
    {
        var order = new CreateOrderDTO
        {
            Items = new List<CreateOrderItemsDTO>
            {
                new CreateOrderItemsDTO
                {
                    ProductId = 1,
                    Quantity = 0
                }
            }
        };

        var result = _validator.Validate(order);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName == "Items[0].Quantity" &&
                error.ErrorMessage ==
                    "Quantity must be between 1 and 1000");
    }

    [Fact]
    public void ItemQuantity_WhenNegative_ShouldFailValidation()
    {
        var order = new CreateOrderDTO
        {
            Items = new List<CreateOrderItemsDTO>
            {
                new CreateOrderItemsDTO
                {
                    ProductId = 1,
                    Quantity = -5
                }
            }
        };

        var result = _validator.Validate(order);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName == "Items[0].Quantity" &&
                error.ErrorMessage ==
                    "Quantity must be between 1 and 1000");
    }

    [Fact]
    public void ItemQuantity_WhenGreaterThanOneThousand_ShouldFailValidation()
    {
        var order = new CreateOrderDTO
        {
            Items = new List<CreateOrderItemsDTO>
            {
                new CreateOrderItemsDTO
                {
                    ProductId = 1,
                    Quantity = 1001
                }
            }
        };

        var result = _validator.Validate(order);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName == "Items[0].Quantity" &&
                error.ErrorMessage ==
                    "Quantity must be between 1 and 1000");
    }

    [Fact]
    public void ProductId_WhenZero_ShouldFailValidation()
    {
        var order = new CreateOrderDTO
        {
            Items = new List<CreateOrderItemsDTO>
            {
                new CreateOrderItemsDTO
                {
                    ProductId = 0,
                    Quantity = 2
                }
            }
        };

        var result = _validator.Validate(order);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName == "Items[0].ProductId" &&
                error.ErrorMessage ==
                    "ProductId Must Be a Positive Number");
    }

    [Fact]
    public void ProductId_WhenNegative_ShouldFailValidation()
    {
        var order = new CreateOrderDTO
        {
            Items = new List<CreateOrderItemsDTO>
            {
                new CreateOrderItemsDTO
                {
                    ProductId = -1,
                    Quantity = 2
                }
            }
        };

        var result = _validator.Validate(order);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            error =>
                error.PropertyName == "Items[0].ProductId" &&
                error.ErrorMessage ==
                    "ProductId Must Be a Positive Number");
    }

    [Fact]
    public void ItemWithMinimumAllowedValues_ShouldPassValidation()
    {
        var order = new CreateOrderDTO
        {
            Items = new List<CreateOrderItemsDTO>
            {
                new CreateOrderItemsDTO
                {
                    ProductId = 1,
                    Quantity = 1
                }
            }
        };

        var result = _validator.Validate(order);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ItemWithMaximumAllowedQuantity_ShouldPassValidation()
    {
        var order = new CreateOrderDTO
        {
            Items = new List<CreateOrderItemsDTO>
            {
                new CreateOrderItemsDTO
                {
                    ProductId = 1,
                    Quantity = 1000
                }
            }
        };

        var result = _validator.Validate(order);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void OrderWithExactlyFiftyItems_ShouldPassValidation()
    {
        var items = Enumerable
            .Range(1, 50)
            .Select(productId =>
                new CreateOrderItemsDTO
                {
                    ProductId = productId,
                    Quantity = 1
                })
            .ToList();

        var order = new CreateOrderDTO
        {
            Items = items
        };

        var result = _validator.Validate(order);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
