namespace ShopAPI.DTOClasses;

public sealed class RestockProductDTO
{
    public int Quantity { get; set; }
    public string Reason { get; set; } = string.Empty;
}
