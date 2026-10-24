using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ShopAPI.Authorization;
using ShopAPI.DTOClasses;
using ShopAPI.Response;
using ShopAPI.Services;

namespace ShopAPI.Controllers
{
    [Authorize]
    [Route("api/Product")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [Authorize(Policy = PolicyNames.AdminOnly)]
        [EnableRateLimiting("PerUserPolicy")]
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<ProductDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<ProductDTO>>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<List<ProductDTO>>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<List<ProductDTO>>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<List<ProductDTO>>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<ProductDTO>>>> GetAllAsync()
        {
            var products = await _productService.GetAllAsync();
            return Ok(new ApiResponse<List<ProductDTO>>
            {
                Data = products,
                IsSuccess = true,
                Message = products.Any() ? "Products retrieved successfully." : "No Products found."
            });
        }

        [Authorize(Policy = PolicyNames.AdminOnly)]
        [HttpPost]
        [EnableRateLimiting("PerUserPolicy")]
        [ProducesResponseType(typeof(ApiResponse<int?>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<int?>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<int?>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<int?>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<int?>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<int?>>> AddNewAsync(CreateProductDTO productDTO)
        {
            var productId = await _productService.AddNewAsync(productDTO);
            if (productId is null)
            {
                return BadRequest(new ApiResponse<int?>
                {
                    Data = null,
                    IsSuccess = false,
                    Message = "Invalid product data. Please ensure all required fields are provided and valid."
                });
            }
            var response = ApiResponse<int?>.SuccessResponse(productId, "Product added successfully.");
            return CreatedAtRoute("GetProductById", new { id = productId }, response);
        }
        
        [AllowAnonymous]
        [HttpGet("{id:int}",Name = "GetProductById")]
        [EnableRateLimiting("PerUserPolicy")]
        [ProducesResponseType(typeof(ApiResponse<ProductDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ProductDTO>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ProductDTO>>> GetByIdAsync([FromRoute] int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ApiResponse<ProductDTO>
                {
                    Data = null,
                    IsSuccess = false,
                    Message = "Product ID must be a positive number."
                });
            }

            var result = await _productService.GetByIdAsync(id);
            if (result == null || (!result.IsActive && !User.IsInRole("Admin")))
            {
                return NotFound(new ApiResponse<ProductDTO>
                {
                    Data = null,
                    IsSuccess = false,
                    Message = "Product does not exist."
                });
            }

            return Ok(new ApiResponse<ProductDTO>
            {
                Data = result,
                IsSuccess = true,
            });
        }

        [Authorize(Policy = PolicyNames.AdminOnly)]
        [HttpPut("{id:int}")]
        [EnableRateLimiting("PerUserPolicy")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateAsync(UpdateProductDTO updateproductDTO, [FromRoute] int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    Data = false,
                    IsSuccess = false,
                    Message = "Product ID must be a positive number."
                });
            }

            var result = await _productService.UpdateAsync(updateproductDTO, id);
            if (!result)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Data = false,
                    IsSuccess = false,
                    Message = "Product does not exist."
                });
            }

            return Ok(new ApiResponse<bool>
            {
                Data = true,
                IsSuccess = true,
                Message = "The product was updated successfully."
            });
        }

        [Authorize(Policy = PolicyNames.AdminOnly)]
        [HttpDelete("{id:int}")]
        [EnableRateLimiting("PerUserPolicy")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteAsync([FromRoute] int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    Data = false,
                    IsSuccess = false,
                    Message = "Product ID must be a positive number."
                });
            }

            var result = await _productService.DeleteAsync(id);
            if (!result)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Data = false,
                    IsSuccess = false,
                    Message = "Product does not exist."
                });
            }

            return Ok(new ApiResponse<bool>
            {
                Data = true,
                IsSuccess = true,
                Message = "Product disabled successfully."
            });
        }

        [Authorize(Policy = PolicyNames.AdminOnly)]
        [HttpPut("{id:int}/disable")]
        [EnableRateLimiting("PerUserPolicy")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<bool>>> DisAbleAsync([FromRoute] int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    Data = false,
                    IsSuccess = false,
                    Message = "Product ID must be a positive number."
                });
            }

            var result = await _productService.DisAbleAsync(id);
            if (!result)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Data = false,
                    IsSuccess = false,
                    Message = "Product does not exist."
                });
            }

            return Ok(new ApiResponse<bool>
            {
                Data = true,
                IsSuccess = true,
                Message = "Product disabled successfully."
            });
        }

        [Authorize(Policy = PolicyNames.AdminOnly)]
        [HttpPost("{id:int}/restock")]
        [EnableRateLimiting("PerUserPolicy")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<bool>>> RestockAsync(int id, [FromBody] RestockProductDTO dto)
        {
            if (id <= 0) return BadRequest(ApiResponse<bool>.FailureResponse("Product ID must be a positive number."));
            var result = await _productService.RestockAsync(id, dto.Quantity, dto.Reason);
            return result
                ? Ok(ApiResponse<bool>.SuccessResponse(true, "Product restocked successfully."))
                : NotFound(ApiResponse<bool>.FailureResponse("Product does not exist or is inactive."));
        }

        [AllowAnonymous]
        [HttpGet("active")]
        [EnableRateLimiting("PerUserPolicy")]
        [ProducesResponseType(typeof(ApiResponse<List<ProductDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<ProductDTO>>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<List<ProductDTO>>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<ProductDTO>>>> GetActiveAsync()
        {
            var products = await _productService.GetActiveAsync();
            return Ok(new ApiResponse<List<ProductDTO>>
            {
                Data = products,
                IsSuccess = true,
                Message = products.Any() ? "Products retrieved successfully." : "No Products found."
            });
        }

        [AllowAnonymous]
        [HttpGet("paged")]
        [EnableRateLimiting("PerUserPolicy")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<object>>> GetActivePagedAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page < 1 || pageSize is < 1 or > 100)
                return BadRequest(ApiResponse<object>.FailureResponse("Page must be positive and pageSize must be between 1 and 100."));
            var result = await _productService.GetActivePageAsync(page, pageSize);
            return Ok(ApiResponse<object>.SuccessResponse(result, "Products retrieved successfully."));
        }
    }
}
