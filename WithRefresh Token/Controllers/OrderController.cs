using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopAPI.Authorization;
using ShopAPI.DTOClasses;
using ShopAPI.Extensions;
using ShopAPI.Response;
using ShopAPI.Services;
using System.IdentityModel.Tokens.Jwt;

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
        private int CurrentPersonId =>
            int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);


        //[Authorize]
        //[HttpGet("whoami")]
        //public IActionResult WhoAmI()
        //{
        //    var claims = User.Claims.Select(c => new { c.Type, c.Value });
        //    return Ok(claims);
        //}

        [Authorize(Policy = PolicyNames.AdminOnly)]
        [HttpGet("AllOrders")]
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

        [Authorize(Policy = PolicyNames.OrderOwner)]
        [HttpGet("{id}")]
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

        [HttpPost("AddOrder")]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<int>>> AddOrder([FromBody] CreateOrderDTO orderDto)
        {
            var result = await _orderService.AddNewAsync(orderDto, CurrentPersonId);

            var currentPersonId = User.GetPersonId();
            if (currentPersonId == null)
                return Unauthorized(new ApiResponse<int>
                {
                    Data = 0,
                    IsSuccess = false,
                    Message = "Person ID Can Not Be Empty ."
                });

            if (result < 0)
            {
                int inx = (-result) - 1;
                return BadRequest(new ApiResponse<int>
                {
                    Data = orderDto.Items[inx].Quantity,
                    IsSuccess = false,
                    Message = "There is not enough stock of product number: " + orderDto.Items[inx].ProductId
                });
            }

            
            return Ok(new ApiResponse<int>
            {
                Data = result,
                IsSuccess = true,
                Message = "Order added successfully."
            });
        }

        [Authorize(Policy = PolicyNames.OrderOwner)]
        [HttpPut("UpdateOrder/{id}")]
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
        [HttpDelete("DeleteOrder/{id}")]
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

            var result = await _orderService.DeleteAsync(id);
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
                Message = "Order deleted successfully."
            });
        }

        [Authorize(Policy = PolicyNames.AdminOnly)]
        [HttpGet("Exists/{id}")]
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
