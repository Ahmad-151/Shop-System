using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ShopAPI.Authorization;
using ShopAPI.DTOClasses;
using ShopAPI.Extensions;
using ShopAPI.Response;
using ShopAPI.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ShopAPI.Controllers
{
    [Authorize]
    [Route("api/Person")]
    [ApiController]
    public class PersonController : ControllerBase
    {
        private readonly IPersonService _personService;
        public PersonController(IPersonService personService)
        {
            _personService = personService;
        }

        [Authorize(Policy = PolicyNames.AdminOnly)]
        [HttpGet]
        [EnableRateLimiting("PerUserPolicy")]
        [ProducesResponseType(typeof(ApiResponse<List<PersonDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<PersonDTO>>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<List<PersonDTO>>>> GetAllAsync()
        {
            var people = await _personService.GetAllAsync();
            return Ok(new ApiResponse<List<PersonDTO>>
            {
                Data = people,
                IsSuccess = true,
                Message = people.Any() ? "Persons retrieved successfully." : "No Persons found."
            });
        }

        [Authorize(Policy = PolicyNames.PersonOwner)]
        [HttpGet("{id:int}")]
        [EnableRateLimiting("PerUserPolicy")]
        [ProducesResponseType(typeof(ApiResponse<PersonDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PersonDTO>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<PersonDTO>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<PersonDTO>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<PersonDTO>>> GetByIdAsync([FromRoute] int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ApiResponse<PersonDTO>
                {
                    Data = null,
                    IsSuccess = false,
                    Message = "Person ID must be a positive number."
                });
            }

            var result = await _personService.GetByIdAsync(id);
            if (result == null)
            {
                return NotFound(new ApiResponse<PersonDTO>
                {
                    Data = null,
                    IsSuccess = false,
                    Message = "Person does not exist."
                });
            }

            return Ok(new ApiResponse<PersonDTO>
            {
                Data = result,
                IsSuccess = true,
            });
        }

        [Authorize(Policy = PolicyNames.PersonOwner)]
        [HttpPut("{id:int}")]
        [EnableRateLimiting("PerUserPolicy")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateAsync(UpdatePersonDTO updatePersonDTO, [FromRoute] int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    Data = false,
                    IsSuccess = false,
                    Message = "Person ID must be a positive number."
                });
            }
            var result = await _personService.UpdateAsync(updatePersonDTO, id);
            if (!result)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Data = false,
                    IsSuccess = false,
                    Message = "Person does not exist."
                });
            }

            return Ok(new ApiResponse<bool>
            {
                Data = true,
                IsSuccess = true,
                Message = "The person was updated successfully."
            });
        }

        [Authorize(Policy = PolicyNames.AdminOnly)]
        [EnableRateLimiting("PerUserPolicy")]
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteAsync([FromRoute] int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    Data = false,
                    IsSuccess = false,
                    Message = "Person ID must be a positive number."
                });
            }
            var result = await _personService.DeleteAsync(id);
            if (!result)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Data = false,
                    IsSuccess = false,
                    Message = "Person does not exist."
                });
            }

            return Ok(new ApiResponse<bool>
            {
                Data = true,
                IsSuccess = true,
                Message = "Person deleted successfully."
            });
        }

        [Authorize(Policy = PolicyNames.AdminOnly)]
        [HttpPost("{id:int}/ban")]
        [EnableRateLimiting("PerUserPolicy")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<bool>>> BannAsync(BannedPersonDTO bannedPersonDTO, [FromRoute] int id)
        {
            if (id <= 0)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    Data = false,
                    IsSuccess = false,
                    Message = "Person ID must be a positive number."
                });
            }

            var actorId = User.GetPersonId();
            if (actorId is null) return Unauthorized(ApiResponse<bool>.FailureResponse("Invalid or missing user identity claim."));
            var result = await _personService.BannAsync(bannedPersonDTO, id, actorId.Value);
            if (!result)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Data = false,
                    IsSuccess = false,
                    Message = "Person does not exist."
                });
            }

            return Ok(new ApiResponse<bool>
            {
                Data = true,
                IsSuccess = true,
                Message = "Person banned successfully."
            });
        }

        [Authorize(Policy = PolicyNames.AdminOnly)]
        [HttpPost("{id:int}/favourite/{DisCount:int}")]
        [EnableRateLimiting("PerUserPolicy")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<bool>>> AddToFavouriteAsync([FromRoute] int DisCount, [FromRoute] int id)
        {
            if (DisCount is <= 0 or > 100 || id <= 0)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    Data = false,
                    IsSuccess = false,
                    Message = "Invalid data. Discount must be between 1 and 100."
                });
            }

            var response = await _personService.AddToFavouriteAsync(DisCount, id);
            if (!response)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Data = false,
                    IsSuccess = false,
                    Message = "Person does not exist."
                });
            }

            return Ok(new ApiResponse<bool>
            {
                Data = true,
                IsSuccess = true,
                Message = "Person added to favourite customers list successfully."
            });
        }
    }
}
