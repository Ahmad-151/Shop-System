namespace ShopAPI.DTOClasses
{
    public class OrderDTO
    {
        public int OrderId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public string Status { get; set; }
        public List<OrderItemsDTO> Items { get; set; } = new List<OrderItemsDTO> { };
    }

    public class CreateOrderDTO
    {
        public List<CreateOrderItemsDTO> Items { get; set; } = new List<CreateOrderItemsDTO> { };
    }

    public class UpdateOrderDTO
    {
        public DateTime? DeliveredAt { get; set; }
        public string Status { get; set; }
    }

    public sealed record CreateOrderResult(int? OrderId, int? OutOfStockProductId = null, int AvailableQuantity = 0)
    {
        public bool Succeeded => OrderId.HasValue;
    }
}
