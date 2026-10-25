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
        public DateTime? CreatedAt { get; set; }
        public List<CreateOrderItemsDTO> Items { get; set; } = new List<CreateOrderItemsDTO> { };
    }

    public class UpdateOrderDTO
    {
        public DateTime? DeliveredAt { get; set; }
        public string Status { get; set; }
    }
}
