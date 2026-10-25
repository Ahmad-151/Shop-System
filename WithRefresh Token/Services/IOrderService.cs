using ShopAPI.DTOClasses;

namespace ShopAPI.Services
{
    public interface IOrderService
    {
        Task<List<OrderDTO>> GetAllAsync();
        Task<OrderDTO?> GetByIdAsync(int id);
        Task<int> AddNewAsync(CreateOrderDTO orderDto, int personId);
        Task<bool> UpdateAsync(UpdateOrderDTO orderDto, int orderId);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
