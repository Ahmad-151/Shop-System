using ShopAPI.DTOClasses;

namespace ShopAPI.Repositories
{
    public interface IOrderRepository
    {
        Task<List<OrderDTO>> GetAllAsync();
        Task<OrderDTO?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(UpdateOrderDTO entity, int orderId);
        Task<int> AddNewAsync(CreateOrderDTO entity, int personId);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<int> GetQuantity(int productId);
        Task<int?> GetOwnerIdAsync(int orderId);
    }
}
