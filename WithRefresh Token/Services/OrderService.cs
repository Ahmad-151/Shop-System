using ShopAPI.DTOClasses;
using ShopAPI.Repositories;

namespace ShopAPI.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<List<OrderDTO>> GetAllAsync()
        {
            return await _orderRepository.GetAllAsync();
        }

        public async Task<OrderDTO?> GetByIdAsync(int id)
        {
            return await _orderRepository.GetByIdAsync(id);
        }
        public async Task<int> AddNewAsync(CreateOrderDTO orderDto, int personId)
        {
            for (int i = 0; i < orderDto.Items.Count; i++)
            {
                int remaining = await _orderRepository.GetQuantity(orderDto.Items[i].ProductId);
                if (remaining < orderDto.Items[i].Quantity)
                {
                    return -(i + 1);
                }
            }

            return await _orderRepository.AddNewAsync(orderDto, personId);
        }

        public async Task<bool> UpdateAsync(UpdateOrderDTO orderDto, int orderId)
        {
            return await _orderRepository.UpdateAsync(orderDto, orderId);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _orderRepository.DeleteAsync(id);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _orderRepository.ExistsAsync(id);
        }
    }
}
