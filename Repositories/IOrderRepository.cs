using ShopAPI.DTOClasses;
using ShopAPI.Response;

namespace ShopAPI.Repositories
{
    public interface IOrderRepository
    {
        Task<List<OrderDTO>> GetAllAsync();
        Task<List<OrderDTO>> GetByPersonIdAsync(int personId);
        Task<PagedResult<OrderDTO>> GetByPersonPageAsync(int personId, int page, int pageSize);
        Task<OrderDTO?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(UpdateOrderDTO entity, int orderId);
        Task<int> AddNewAsync(CreateOrderDTO entity, int personId);
        Task<int> AddNewAsync(CreateOrderDTO entity, int personId, string idempotencyKey, string requestHash);
        Task<bool> DeleteAsync(int id);
        Task<bool> CancelAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<int> GetQuantity(int productId);
        Task<int?> GetOwnerIdAsync(int orderId);
    }
}
