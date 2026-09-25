namespace ShopAPI.DTOClasses
{
    // يمثّل عنصراً ضمن طلب مُرجَع للعميل (Response) - يتضمّن ProductId
    // حتى يستطيع العميل ربط كل سطر بالمنتج الذي يخصّه.
    public class OrderItemsDTO
    {
        public int OrderItemId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    // يمثّل عنصراً مُرسَلاً من العميل عند إنشاء طلب جديد.
    // لا نثق بـ UnitPrice القادم من العميل - السعر الحقيقي يُقرأ من Products
    // داخل الإجراء المخزّن (راجع AddListOfOrderItemToOrder) لمنع التلاعب بالسعر.
    public class CreateOrderItemsDTO
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
