namespace ShopAPI.DTOClasses
{
    public class OrderItemsDTO
    {
        public int OrderItemId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
    public class CreateOrderItemsDTO
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
