using ShopAPI.DTOClasses;
using ShopAPI.Repositories;
using ShopAPI.Response;
using ShopAPI.Infrastructure;

namespace ShopAPI.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<List<OrderDTO>> GetAllAsync() => await _orderRepository.GetAllAsync();
        public async Task<List<OrderDTO>> GetByPersonIdAsync(int personId) => await _orderRepository.GetByPersonIdAsync(personId);
        
        public Task<PagedResult<OrderDTO>> GetByPersonPageAsync(int personId, int page, int pageSize) 
            => _orderRepository.GetByPersonPageAsync(personId, page, pageSize);

        public async Task<OrderDTO?> GetByIdAsync(int id) => await _orderRepository.GetByIdAsync(id);

        public Task<CreateOrderResult> AddNewAsync(CreateOrderDTO orderDto, int personId)
            => AddNewCoreAsync(orderDto, personId, null);

        public Task<CreateOrderResult> AddNewAsync(CreateOrderDTO orderDto, int personId, string idempotencyKey)
            => AddNewCoreAsync(orderDto, personId, idempotencyKey);

        private async Task<CreateOrderResult> AddNewCoreAsync(CreateOrderDTO orderDto, int personId, string? idempotencyKey)
        {
            if (idempotencyKey is not null)
            {
                var orderId = await _orderRepository.AddNewAsync(
                    orderDto,
                    personId,
                    idempotencyKey,
                    IdempotencyKeyHasher.Compute(orderDto));
                return new CreateOrderResult(orderId);
            }


            foreach (var item in orderDto.Items)
            {
                int remaining = await _orderRepository.GetQuantity(item.ProductId);
                if (remaining < item.Quantity)
                    return new CreateOrderResult(null, item.ProductId, remaining);
            }

            return new CreateOrderResult(await _orderRepository.AddNewAsync(orderDto, personId));
        }

        public async Task<bool> UpdateAsync(UpdateOrderDTO orderDto, int orderId)
        {
            return await _orderRepository.UpdateAsync(orderDto, orderId);
        }

        public Task<bool> DeleteAsync(int id) => _orderRepository.DeleteAsync(id);

        public Task<bool> CancelAsync(int id) => _orderRepository.CancelAsync(id);

        public async Task<bool> ExistsAsync(int id)
        {
            return await _orderRepository.ExistsAsync(id);
        }
    }
}
