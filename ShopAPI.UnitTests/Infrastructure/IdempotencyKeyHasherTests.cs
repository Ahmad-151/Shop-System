using ShopAPI.DTOClasses;
using ShopAPI.Infrastructure;

namespace ShopAPI.UnitTests.Infrastructure;

public class IdempotencyKeyHasherTests
{
    [Fact]
    public void Compute_ShouldBeDeterministicAndIndependentOfItemOrder()
    {
        var first = new CreateOrderDTO
        {
            Items = new List<CreateOrderItemsDTO>
            {
                new() { ProductId = 2, Quantity = 3 },
                new() { ProductId = 1, Quantity = 2 }
            }
        };

        var second = new CreateOrderDTO
        {
            Items = new List<CreateOrderItemsDTO>
            {
                new() { ProductId = 1, Quantity = 2 },
                new() { ProductId = 2, Quantity = 3 }
            }
        };

        Assert.Equal(IdempotencyKeyHasher.Compute(first), IdempotencyKeyHasher.Compute(second));
        Assert.Equal(64, IdempotencyKeyHasher.Compute(first).Length);
    }

    [Fact]
    public void Compute_ShouldNormalizeDuplicateProductLines()
    {
        var split = new CreateOrderDTO
        {
            Items = new List<CreateOrderItemsDTO>
            {
                new() { ProductId = 1, Quantity = 2 },
                new() { ProductId = 1, Quantity = 3 }
            }
        };

        var merged = new CreateOrderDTO
        {
            Items = new List<CreateOrderItemsDTO>
            {
                new() { ProductId = 1, Quantity = 5 }
            }
        };

        Assert.Equal(IdempotencyKeyHasher.Compute(split), IdempotencyKeyHasher.Compute(merged));
    }
}
