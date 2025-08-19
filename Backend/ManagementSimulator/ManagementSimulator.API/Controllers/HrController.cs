using ManagementSimulator.Core.Dtos.Requests.Users;
using ManagementSimulator.Core.Dtos.Responses.PagedResponse;
using ManagementSimulator.Core.Dtos.Responses.User;
using ManagementSimulator.Core.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ManagementSimulator.Infrastructure.Exceptions;

namespace ManagementSimulator.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class HrController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<HrController> _logger;

        public HrController(IUserService userService, ILogger<HrController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet("users")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllUsersForHrAsync(
            [FromQuery] int? year = null,
            [FromQuery] int? page = null,
            [FromQuery] int? pageSize = null,
            [FromQuery] string? department = null)
        {
            try
            {
                var request = new HrUsersRequestDto
                {
                    Year = year ?? DateTime.Now.Year,
                    Page = page ?? 1,
                    PageSize = pageSize ?? 10,
                    Department = department
                };

                var result = await _userService.GetAllUsersForHrAsync(request);

                if (result.Data == null || !result.Data.Any())
                {
                    return NotFound(new
                    {
                        Message = "No users found matching the specified criteria.",
                        Data = new PagedResponseDto<HrUserResponseDto>
                        {
                            Data = new List<HrUserResponseDto>(),
                            TotalCount = 0,
                            Page = request.Page ?? 1,
                            PageSize = request.PageSize ?? 10,
                            TotalPages = 0
                        },
                        Success = false,
                        Timestamp = DateTime.UtcNow
                    });
                }

                return Ok(new
                {
                    Message = "Users with leave information retrieved successfully.",
                    Data = result,
                    Success = true,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving users for HR");
                return StatusCode(500, new
                {
                    Message = "An error occurred while retrieving user data.",
                    Data = (object?)null,
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpGet("users/{userId}/leave-summary")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserLeaveSummaryAsync(int userId, [FromQuery] int? year = null)
        {
            try
            {
                var currentYear = year ?? DateTime.Now.Year;
                var request = new HrUsersRequestDto
                {
                    Year = currentYear,
                    Page = 1,
                    PageSize = 10
                };

                var result = await _userService.GetAllUsersForHrAsync(request);
                var user = result.Data?.FirstOrDefault(u => u.Id == userId);

                if (user == null)
                {
                    return NotFound(new
                    {
                        Message = $"User with ID {userId} not found.",
                        Data = (object?)null,
                        Success = false,
                        Timestamp = DateTime.UtcNow
                    });
                }

                return Ok(new
                {
                    Message = "User leave summary retrieved successfully.",
                    Data = new
                    {
                        UserId = user.Id,
                        UserName = user.FullName,
                        Year = currentYear,
                        TotalLeaveDays = user.TotalLeaveDays,
                        UsedLeaveDays = user.UsedLeaveDays,
                        RemainingLeaveDays = user.RemainingLeaveDays,
                        LeaveTypeStatistics = user.LeaveTypeStatistics
                    },
                    Success = true,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving leave summary for user {UserId}", userId);
                return StatusCode(500, new
                {
                    Message = "An error occurred while retrieving leave summary.",
                    Data = (object?)null,
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpPost("users/{id}/vacation")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AdjustUserVacationAsync(int id, [FromBody] HrAdjustVacationRequestDto request)
        {
            try
            {
                if (id != request.Id)
                {
                    return BadRequest(new
                    {
                        Message = "Route id and body id mismatch.",
                        Data = (object?)null,
                        Success = false,
                        Timestamp = DateTime.UtcNow
                    });
                }

                var newVacation = await _userService.AdjustUserVacationAsync(request.Id, request.Days);

                return Ok(new
                {
                    Message = "Vacation days adjusted successfully.",
                    Data = new { UserId = request.Id, NewVacation = newVacation },
                    Success = true,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (EntryNotFoundException)
            {
                return NotFound(new
                {
                    Message = $"User with ID {id} not found.",
                    Data = (object?)null,
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adjusting vacation for user {UserId}", id);
                return StatusCode(500, new
                {
                    Message = "An error occurred while adjusting vacation days.",
                    Data = (object?)null,
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }
        }
    }
}