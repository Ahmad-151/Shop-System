using ShopAPI.DTOClasses;
using System.Security.Cryptography;
using System.Text;

namespace ShopAPI.Infrastructure;

public static class IdempotencyKeyHasher
{
    public static string Compute(CreateOrderDTO order)
    {
        var canonical = string.Join("|", order.Items
            .GroupBy(item => item.ProductId)
            .Select(group => new
            {
                ProductId = group.Key,
                Quantity = group.Sum(item => item.Quantity)
            })
            .OrderBy(item => item.ProductId)
            .Select(item => $"{item.ProductId}:{item.Quantity}"));

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }
}
