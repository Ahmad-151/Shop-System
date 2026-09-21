using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopAPI.DTOClasses;
using ShopAPI.Response;
using ShopAPI.Services;

namespace ShopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("Register")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<string>>> RegisterAsync([FromBody] RegisterPersonDTO registerDto)
        {
            var token = await _authService.RegisterAsync(registerDto);

            if (token == null)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Data = null,
                    IsSuccess = false,
                    Message = "This phone number is already registered."
                });
            }

            return Ok(new ApiResponse<string>
            {
                Data = token,
                IsSuccess = true,
                Message = "Account created successfully."
            });
        }

        [HttpPost("Login")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<string>>> LoginAsync([FromBody] LoginDTO loginDto)
        {
            var token = await _authService.LoginAsync(loginDto);

            if (token == null)
            {
                return Unauthorized(new ApiResponse<string>
                {
                    Data = null,
                    IsSuccess = false,
                    Message = "Invalid phone number or password."
                });
            }

            return Ok(new ApiResponse<string>
            {
                Data = token,
                IsSuccess = true,
                Message = "Login successful."
            });
        }
    }
}
