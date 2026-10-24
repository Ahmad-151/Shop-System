using ShopAPI.DTOClasses;
using ShopAPI.Response;

namespace ShopAPI.Services
{
    public interface IOrderService
    {
        Task<List<OrderDTO>> GetAllAsync();
        Task<List<OrderDTO>> GetByPersonIdAsync(int personId);
        Task<PagedResult<OrderDTO>> GetByPersonPageAsync(int personId, int page, int pageSize);
        Task<OrderDTO?> GetByIdAsync(int id);
        Task<CreateOrderResult> AddNewAsync(CreateOrderDTO orderDto, int personId);
        Task<CreateOrderResult> AddNewAsync(CreateOrderDTO orderDto, int personId, string idempotencyKey);
        Task<bool> UpdateAsync(UpdateOrderDTO orderDto, int orderId);
        Task<bool> DeleteAsync(int id);
        Task<bool> CancelAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
