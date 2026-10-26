using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopAPI.Authorization;
using ShopAPI.DTOClasses;
using ShopAPI.Response;
using ShopAPI.Services;

namespace ShopAPI.Controllers;

[ApiController]
[Route("api/Category")]
public sealed class CategoryController : ControllerBase
{
    private readonly ICategoryService _service;

    public CategoryController(ICategoryService service) => _service = service;

    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<CategoryDTO>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<CategoryDTO>>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<List<CategoryDTO>>>> GetAllAsync()
    {
        return Ok(ApiResponse<List<CategoryDTO>>.SuccessResponse(await _service.GetAllAsync(), "Categories retrieved successfully."));
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CategoryDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CategoryDTO>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<CategoryDTO>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<CategoryDTO>>> GetByIdAsync(int id)
    {
        if (id <= 0) return BadRequest(ApiResponse<CategoryDTO>.FailureResponse("Category ID must be a positive number."));
        var category = await _service.GetByIdAsync(id);
        return category is null
            ? NotFound(ApiResponse<CategoryDTO>.FailureResponse("Category does not exist."))
            : Ok(ApiResponse<CategoryDTO>.SuccessResponse(category, "Category retrieved successfully."));
    }

    [Authorize(Policy = PolicyNames.AdminOnly)]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<int?>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<int?>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<int?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<int?>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<int?>>> CreateAsync([FromBody] CategoryDTO dto)
    {
        var id = await _service.CreateAsync(dto.CategoryName);
        if (id is null)
            return BadRequest(ApiResponse<int>.FailureResponse("Category name can not be empty ."));

        return StatusCode(StatusCodes.Status201Created, ApiResponse<int?>.SuccessResponse(id, "Category created successfully."));
    }

    [Authorize(Policy = PolicyNames.AdminOnly)]
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateAsync(int id, [FromBody] CategoryDTO dto)
    {
        if (id <= 0) return BadRequest(ApiResponse<bool>.FailureResponse("Category ID must be a positive number."));
        var updated = await _service.UpdateAsync(id, dto.CategoryName);
        return updated
            ? Ok(ApiResponse<bool>.SuccessResponse(true, "Category updated successfully."))
            : NotFound(ApiResponse<bool>.FailureResponse("Category does not exist."));
    }

    [Authorize(Policy = PolicyNames.AdminOnly)]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteAsync(int id)
    {
        if (id <= 0) return BadRequest(ApiResponse<bool>.FailureResponse("Category ID must be a positive number."));
        var deleted = await _service.DeleteAsync(id);
        return deleted
            ? Ok(ApiResponse<bool>.SuccessResponse(true, "Category deleted successfully."))
            : NotFound(ApiResponse<bool>.FailureResponse("Category does not exist."));
    }
}
