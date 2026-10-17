using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ShopAPI.Authorization;
using ShopAPI.DTOClasses;
using ShopAPI.Extensions;
using ShopAPI.Response;
using ShopAPI.Services;

namespace ShopAPI.Controllers
{
    [Authorize]
    [Route("api/Order")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [Authorize(Policy = PolicyNames.AdminOnly)]
        [HttpGet]
        [EnableRateLimiting("AdminOperationsPolicy")]
        [ProducesResponseType(typeof(ApiResponse<List<OrderDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<OrderDTO>>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<List<OrderDTO>>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<List<OrderDTO>>>> GetAllOrders()
        {
            var orders = await _orderService.GetAllAsync();
            return Ok(new ApiResponse<List<OrderDTO>>
            {
                Data = orders,
                IsSuccess = true,
                Message = orders.Any() ? "Orders retrieved successfully." : "No orders found."
            });
        }

        [HttpGet("mine/paged")]
        [EnableRateLimiting("PerUserPolicy")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<object>>> GetMyOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var personId = User.GetPersonId();
            if (personId is null)
            {
                return Unauthorized(ApiResponse<object>.FailureResponse("Invalid or missing user identity claim."));
            }
            if (page < 1 || pageSize is < 1 or > 100)
                return BadRequest(ApiResponse<object>.FailureResponse("Page must be positive and pageSize must be between 1 and 100."));
            var orders = await _orderService.GetByPersonPageAsync(personId.Value, page, pageSize);
            return Ok(ApiResponse<object>.SuccessResponse(orders, orders.TotalCount == 0 ? "No orders found." : "Orders retrieved successfully."));
        }

        [Authorize(Policy = PolicyNames.OrderOwner)]
        [HttpGet("{id:int}")]
        [EnableRateLimiting("PerUserPolicy")]
        [ProducesResponseType(typeof(ApiResponse<OrderDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<OrderDTO>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<OrderDTO>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<OrderDTO>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<OrderDTO>>> GetOrderById([FromRoute] int id)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order == null)
            {
                return NotFound(new ApiResponse<OrderDTO>
                {
                    Data = null,
                    IsSuccess = false,
                    Message = "Order not found."
                });
            }
            return Ok(new ApiResponse<OrderDTO>
            {
                Data = order,
                IsSuccess = true,
                Message = "Order retrieved successfully."
            });
        }

        [HttpPost]
        [EnableRateLimiting("OrderCreationPolicy")]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<int>>> AddOrder([FromBody] CreateOrderDTO orderDto, [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey)
        {
            var personId = User.GetPersonId();
            if (personId is null)
            {
                return Unauthorized(new ApiResponse<int>
                {
                    Data = 0,
                    IsSuccess = false,
                    Message = "Invalid or missing user identity claim."
                });
            }

            if (string.IsNullOrWhiteSpace(idempotencyKey) || idempotencyKey.Length > 100)
                return BadRequest(ApiResponse<int>.FailureResponse("Idempotency-Key header is required and must not exceed 100 characters."));

            var result = await _orderService.AddNewAsync(orderDto, personId.Value, idempotencyKey);

            if (!result.Succeeded)
            {
                return Conflict(ApiResponse<int>.FailureResponse("Insufficient stock.", new List<string>
                {
                    $"ProductId={result.OutOfStockProductId}",
                    $"AvailableQuantity={result.AvailableQuantity}"
                }));
            }

            var response = ApiResponse<int>.SuccessResponse(result.OrderId!.Value, "Order added successfully.");
            return CreatedAtAction(nameof(GetOrderById), new { id = result.OrderId.Value }, response);
        }

        [Authorize(Policy = PolicyNames.AdminOnly)]
        [HttpPut("{id:int}")]
        [EnableRateLimiting("PerUserPolicy")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateOrder([FromBody] UpdateOrderDTO orderDto, [FromRoute] int id)
        {
            var result = await _orderService.UpdateAsync(orderDto, id);
            if (!result)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Data = false,
                    IsSuccess = false,
                    Message = "Order not found."
                });
            }
            return Ok(new ApiResponse<bool>
            {
                Data = true,
                IsSuccess = true,
                Message = "Order updated successfully."
            });
        }

        [Authorize(Policy = PolicyNames.OrderOwner)]
        [HttpPost("{id:int}/cancel")]
        [EnableRateLimiting("PerUserPolicy")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteOrder([FromRoute] int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    Data = false,
                    IsSuccess = false,
                    Message = "Invalid order ID."
                });
            }

            var result = await _orderService.CancelAsync(id);
            if (!result)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Data = false,
                    IsSuccess = false,
                    Message = "Order not found."
                });
            }
            return Ok(new ApiResponse<bool>
            {
                Data = true,
                IsSuccess = true,
                Message = "Order cancelled and stock restored successfully."
            });
        }

        [Authorize(Policy = PolicyNames.AdminOnly)]
        [HttpGet("{id:int}/exists")]
        [EnableRateLimiting("PerUserPolicy")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<bool>>> Exists([FromRoute] int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    Data = false,
                    IsSuccess = false,
                    Message = "Invalid order ID."
                });
            }

            var exists = await _orderService.ExistsAsync(id);
            if (!exists)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Data = false,
                    IsSuccess = false,
                    Message = "Order not found."
                });
            }
            return Ok(new ApiResponse<bool>
            {
                Data = true,
                IsSuccess = true,
                Message = "Order exists."
            });
        }
    }
}
