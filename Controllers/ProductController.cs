using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopAPI.DTOClasses;
using ShopAPI.Response;
using ShopAPI.Services;

namespace ShopAPI.Controllers
{
    
    [Route("api/Product")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet("AllProducts")]
        [ProducesResponseType(typeof(ApiResponse<List<ProductDTO>>), StatusCodes.Status200OK)]
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
        [HttpPost("AddNewProduct")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<int>>> AddNewAsync(CreateProductDTO productDTO)
        {
            return Ok(new ApiResponse<int>
            {
                Data = await _productService.AddNewAsync(productDTO),
                IsSuccess = true,
                Message = "Product added successfully."
            });
        }

        [HttpGet("GetProduct/{id}")]
        [ProducesResponseType(typeof(ApiResponse<ProductDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ProductDTO>), StatusCodes.Status404NotFound)]
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
            if (result == null)
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

        [HttpPut("UpdateProduct/{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
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

        [HttpDelete("DeleteProduct/{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
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
                Message = "Product deleted successfully."
            });
        }

        [HttpPut("DisAbleProduct/{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
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

        [HttpGet("AllActiveProducts")]
        [ProducesResponseType(typeof(ApiResponse<List<ProductDTO>>), StatusCodes.Status200OK)]
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
    }
}
