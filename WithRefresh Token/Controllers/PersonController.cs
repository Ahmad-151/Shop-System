using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        private int CurrentPersonId =>
        User.GetPersonId() ?? throw new UnauthorizedAccessException("Invalid or missing user identity claim.");

        [Authorize(Policy = PolicyNames.AdminOnly)]
        [HttpGet("AllPeople")]
        [ProducesResponseType(typeof(ApiResponse<List<PersonDTO>>), StatusCodes.Status200OK)]
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
        [HttpGet("GetPerson/{id}")]
        [ProducesResponseType(typeof(ApiResponse<PersonDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PersonDTO>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<PersonDTO>), StatusCodes.Status403Forbidden)]
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
        [HttpPut("UpdatePerson/{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status403Forbidden)]
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
        [HttpDelete("DeletePerson/{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status403Forbidden)]
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
        [HttpPost("BannPerson/{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
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

            var result = await _personService.BannAsync(bannedPersonDTO, id);
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
        [HttpPost("AddToFavourites/{DisCount}/{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<bool>>> AddToFavouriteAsync([FromRoute] int DisCount, [FromRoute] int id)
        {
            if (DisCount <= 0 || id <= 0)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    Data = false,
                    IsSuccess = false,
                    Message = "Invalid data."
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
