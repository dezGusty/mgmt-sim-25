using ManagementSimulator.Core.Dtos.Requests.Users;
using ManagementSimulator.Core.Dtos.Responses.PagedResponse;
using ManagementSimulator.Core.Dtos.Responses.User;
using ManagementSimulator.Core.Dtos.Responses.Users;
using ManagementSimulator.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace ManagementSimulator.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllUsersAsync()
        {
            var users = await _userService.GetAllUsersAsync();
            if (users == null || !users.Any())
            {
                return NotFound(new
                {
                    Message = "No users found.",
                    Data = new List<UserResponseDto>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Message = "Users retrieved successfully.",
                Data = users,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("managers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllManagersFilteredAsync([FromQuery] QueriedUserRequestDto payload)
        {
            PagedResponseDto<UserResponseDto>? managers = await _userService.GetAllManagersFilteredAsync(payload);

            if (managers.Data == null || !managers.Data.Any())
            {
                return NotFound(new
                {
                    Message = "No managers found.",
                    Data = new List<UserResponseDto>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Message = "Managers retrieved successfully.",
                Data = managers,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("unassignedUsers/queried")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllUnassignedUsersFilteredAsync([FromQuery] QueriedUserRequestDto payload)
        {
            var unassignedUsers = await _userService.GetAllUnassignedUsersFilteredAsync(payload);
            if (unassignedUsers == null || unassignedUsers.Data == null || !unassignedUsers.Data.Any())
            {
                return NotFound(new
                {
                    Message = "No unassigned users found.",
                    Data = new List<UserResponseDto>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Message = "Unassigned users retrieved successfully.",
                Data = unassignedUsers,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admins")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllAdminsAsync([FromQuery] string? lastName, [FromQuery] string? email)
        {
            var admins = await _userService.GetAllAdminsAsync(lastName, email);
            if (admins == null || !admins.Any())
            {
                return NotFound(new
                {
                    Message = "No admin users found.",
                    Data = new List<UserResponseDto>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Message = "Admin users retrieved successfully.",
                Data = admins,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admins/queried")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllAdminsFilteredAsync([FromQuery] QueriedUserRequestDto payload)
        {
            var admins = await _userService.GetAllAdminsFilteredAsync(payload);
            if (admins == null || admins.Data == null || !admins.Data.Any())
            {
                return NotFound(new
                {
                    Message = "No filtered admin users found.",
                    Data = new List<UserResponseDto>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Message = "Filtered admin users retrieved successfully.",
                Data = admins,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("queried")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllUsersFilteredAsync([FromQuery] QueriedUserRequestDto payload)
        {
            var users = await _userService.GetAllUsersFilteredAsync(payload);
            if (users == null || users.Data == null || !users.Data.Any())
            {
                return NotFound(new
                {
                    Message = "No filtered users found.",
                    Data = new List<UserResponseDto>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Message = "Filtered users retrieved successfully.",
                Data = users,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("includeRelationships")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllUsersWithRelationshipsAsync()
        {
            var users = await _userService.GetAllUsersIncludeRelationshipsAsync();
            if (users == null || !users.Any())
            {
                return NotFound(new
                {
                    Message = "No users with relationships found.",
                    Data = new List<UserResponseDto>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Message = "Users with relationships retrieved successfully.",
                Data = users,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("includeRelationships/queried")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllUsersWithRelationshipsFilteredAsync([FromQuery] QueriedUserRequestDto payload)
        {
            var users = await _userService.GetAllUsersIncludeRelationshipsFilteredAsync(payload);
            if (users.Data == null || !users.Data.Any())
            {
                return NotFound(new
                {
                    Message = "No filtered users with relationships found.",
                    Data = new List<UserResponseDto>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Message = "Filtered users with relationships retrieved successfully.",
                Data = users,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserByIdAsync(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(new
                {
                    Message = $"User with ID {id} not found.",
                    Data = new List<UserResponseDto>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Message = "User retrieved successfully.",
                Data = user,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddUserAsync([FromBody] CreateUserRequestDto dto)
        {
            var user = await _userService.AddUserAsync(dto);
            return Created($"/api/users/{user.Id}", new
            {
                Message = "User created successfully.",
                Data = user,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUserAsync(int id, [FromBody] UpdateUserRequestDto dto)
        {
            var updatedUser = await _userService.UpdateUserAsync(id, dto);
            if (updatedUser == null)
            {
                return NotFound(new
                {
                    Message = $"User with ID {id} not found.",
                    Data = new List<UserResponseDto>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Message = "User updated successfully.",
                Data = updatedUser,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteUserAsync(int id)
        {
            bool result = await _userService.DeleteUserAsync(id);
            if (!result)
            {
                return NotFound(new
                {
                    Message = $"User with ID {id} not found.",
                    Data = new List<UserResponseDto>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Message = $"User with ID {id} deleted successfully.",
                Data = new List<UserResponseDto>(),
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/restore")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RestoreUserAsync(int id)
        {
            await _userService.RestoreUserByIdAsync(id);

            return Ok(new
            {
                Message = $"User with ID {id} restored successfully.",
                Data = new List<UserResponseDto>(),
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admins/count")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTotalAdminsCountAsync()
        {
            var count = await _userService.GetTotalAdminsCountAsync();
            return Ok(new
            {
                Message = "Total admins count retrieved successfully.",
                Data = count,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("managers/count")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTotalManagersCountAsync()
        {
            var count = await _userService.GetTotalManagersCountAsync();
            return Ok(new
            {
                Message = "Total managers count retrieved successfully.",
                Data = count,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("unassignedUsers/count")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTotalUnassignedUsersCountAsync()
        {
            var count = await _userService.GetTotalUnassignedUsersCountAsync();
            return Ok(new
            {
                Message = "Total unassigned users count retrieved successfully.",
                Data = count,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("globalSearch")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GlobalSearchAsync([FromQuery] GlobalSearchRequestDto request)
        {
            var result = await _userService.GlobalSearchAsync(request);

            var hasData = (result.Managers?.Data?.Any() == true) ||
                         (result.Admins?.Data?.Any() == true) ||
                             (result.UnassignedUsers?.Data?.Any() == true);

                if (!hasData && !string.IsNullOrEmpty(request.GlobalSearch))
                {
                    return NotFound(new
                    {
                        Message = $"No results found for search term '{request.GlobalSearch}'.",
                        Data = result,
                        Success = false,
                        Timestamp = DateTime.UtcNow
                    });
                }

                return Ok(new
                {
                    Message = "Global search completed successfully.",
                    Data = result,
                    Success = true,
                    Timestamp = DateTime.UtcNow
                });
            }
       }
    }
