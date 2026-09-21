namespace ShopAPI.DTOClasses
{
    public class ProductDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int CategoryId { get; set; }
        public int RemainingQuantity { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateProductDTO
    {
        public string ProductName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int CategoryId { get; set; }
    }

    public class UpdateProductDTO
    {
        public string Description { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
