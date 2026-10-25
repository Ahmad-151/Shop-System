using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        private const string RefreshCookieName = "refreshToken";
        private const string RefreshCookiePath = "/api/Auth";
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("Register")]
        public async Task<ActionResult<ApiResponse<string>>> RegisterAsync(
            [FromBody] RegisterPersonDTO registerDto)
        {
            var tokens = await _authService.RegisterAsync(registerDto);
            if (tokens is null)
            {
                return BadRequest(ApiResponse<string>.FailureResponse(
                    "This phone number is already registered."));
            }

            SetRefreshTokenCookie(
                tokens.RefreshToken,
                tokens.RefreshTokenExpiresAt);

            return Ok(ApiResponse<string>.SuccessResponse(
                tokens.AccessToken,
                "Account created successfully."));
        }

        [HttpPost("Login")]
        public async Task<ActionResult<ApiResponse<string>>> LoginAsync(
            [FromBody] LoginDTO loginDto)
        {
            var tokens = await _authService.LoginAsync(loginDto);
            if (tokens is null)
            {
                return Unauthorized(ApiResponse<string>.FailureResponse(
                    "Invalid phone number or password."));
            }

            SetRefreshTokenCookie(
                tokens.RefreshToken,
                tokens.RefreshTokenExpiresAt);

            return Ok(ApiResponse<string>.SuccessResponse(
                tokens.AccessToken,
                "Login successful."));
        }

        [HttpPost("RefreshToken")]
        public async Task<ActionResult<ApiResponse<string>>>
            RefreshTokenAsync()
        {
            var oldToken = Request.Cookies[RefreshCookieName];
            if (string.IsNullOrWhiteSpace(oldToken))
            {
                return Unauthorized(ApiResponse<string>.FailureResponse(
                    "Refresh token is missing."));
            }

            var tokens = await _authService
                .RefreshTokenAsync(oldToken);

            if (tokens is null)
            {
                DeleteRefreshTokenCookie();
                return Unauthorized(ApiResponse<string>.FailureResponse(
                    "Invalid or expired session. Please login again."));
            }

            SetRefreshTokenCookie(
                tokens.RefreshToken,
                tokens.RefreshTokenExpiresAt);

            return Ok(ApiResponse<string>.SuccessResponse(
                tokens.AccessToken,
                "Token refreshed successfully."));
        }

        [HttpPost("Logout")]
        public async Task<IActionResult> LogoutAsync()
        {
            var refreshToken = Request.Cookies[RefreshCookieName];
            await _authService.LogoutAsync(refreshToken);
            DeleteRefreshTokenCookie();

            return Ok(ApiResponse<string>.SuccessResponse(
                (string)null,
                "Logged out successfully."));
        }

        private void SetRefreshTokenCookie(
            string refreshToken,
            DateTime expires)
        {
            Response.Cookies.Append(
                RefreshCookieName,
                refreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = new DateTimeOffset(expires),
                    Path = RefreshCookiePath
                });
        }

        private void DeleteRefreshTokenCookie()
        {
            Response.Cookies.Delete(
                RefreshCookieName,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Path = RefreshCookiePath
                });
        }
    }
}
