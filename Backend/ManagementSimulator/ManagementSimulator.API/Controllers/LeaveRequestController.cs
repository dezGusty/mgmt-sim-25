using Azure.Core;
using ManagementSimulator.Core.Dtos.Requests.LeaveRequest;
using ManagementSimulator.Core.Dtos.Requests.LeaveRequests;
using ManagementSimulator.Core.Dtos.Responses;
using ManagementSimulator.Core.Dtos.Responses.LeaveRequest;
using ManagementSimulator.Core.Services;
using ManagementSimulator.Core.Services.Interfaces;
using ManagementSimulator.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ManagementSimulator.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LeaveRequestsController : ControllerBase
    {
        private readonly ILeaveRequestService _leaveRequestService;
        private readonly IResourceAuthorizationService _resourceAuthorizationService;
        private readonly ILogger<LeaveRequestsController> _logger;

        public LeaveRequestsController(ILeaveRequestService leaveRequestService, IResourceAuthorizationService resourceAuthorizationService, ILogger<LeaveRequestsController> logger)
        {
            _leaveRequestService = leaveRequestService;
            _resourceAuthorizationService = resourceAuthorizationService;
            _logger = logger;
        }
        
        [Authorize(Roles = "Manager")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddLeaveRequestAsync([FromBody] CreateLeaveRequestRequestDto dto)
        {
            try
            {
                var leaveRequest = await _leaveRequestService.AddLeaveRequestAsync(dto);
                return Created($"api/LeaveRequests/{leaveRequest.Id}", new
                {
                    Message = "Leave request created successfully.",
                    Data = leaveRequest,
                    Success = true,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (LeaveRequestOverlapException ex)
            {
                return BadRequest(new
                {
                    Message = ex.Message,
                    Data = (object?)null,
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (InvalidDateRangeException ex)
            {
                return BadRequest(new
                {
                    Message = ex.Message,
                    Data = (object?)null,
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (InsufficientLeaveDaysException ex)
            {
                return BadRequest(new
                {
                    Message = ex.Message,
                    Data = new
                    {
                        UserId = ex.UserId,
                        LeaveRequestTypeId = ex.LeaveRequestTypeId,
                        RequestedDays = ex.RequestedDays,
                        RemainingDays = ex.RemainingDays
                    },
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        [Authorize(Roles = "Employee")]
        [HttpPost("by-employee")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddLeaveRequestByEmployeeAsync([FromBody] CreateLeaveRequestByEmployeeDto dto)
        {
            var nameIdentifierClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(nameIdentifierClaim))
            {
                return Unauthorized(new
                {
                    Message = "User ID is missing from the token.",
                    Data = new List<LeaveRequestResponseDto>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            if (!int.TryParse(nameIdentifierClaim, out var userId))
            {
                return BadRequest(new
                {
                    Message = "Invalid User ID.",
                    Data = new List<LeaveRequestResponseDto>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            try
            {
                var leaveRequest = await _leaveRequestService.AddLeaveRequestByEmployeeAsync(dto, userId);

                return Created($"api/LeaveRequests/{leaveRequest.Id}", new
                {
                    Message = "Leave request created successfully.",
                    Data = leaveRequest,
                    Success = true,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (LeaveRequestOverlapException ex)
            {
                return BadRequest(new
                {
                    Message = ex.Message,
                    Data = (object?)null,
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (InvalidDateRangeException ex)
            {
                return BadRequest(new
                {
                    Message = ex.Message,
                    Data = (object?)null,
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (InsufficientLeaveDaysException ex)
            {
                return BadRequest(new
                {
                    Message = ex.Message,
                    Data = new
                    {
                        UserId = ex.UserId,
                        LeaveRequestTypeId = ex.LeaveRequestTypeId,
                        RequestedDays = ex.RequestedDays,
                        RemainingDays = ex.RemainingDays
                    },
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }
        }


        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLeaveRequestByIdAsync(int id)
        {
            var nameIdentifierClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(nameIdentifierClaim) || !int.TryParse(nameIdentifierClaim, out var currentUserId))
            {
                return Unauthorized(new
                {
                    Message = "User ID is missing from the token.",
                    Data = new LeaveRequestResponseDto(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            // Check authorization unless user is admin
            if (!User.IsInRole("Admin"))
            {
                var canAccess = await _resourceAuthorizationService.CanManagerAccessLeaveRequestAsync(currentUserId, id);
                if (!canAccess)
                {
                    return Forbid();
                }
            }

            var request = await _leaveRequestService.GetRequestByIdAsync(id);
            if (request == null)
            {
                return NotFound(new
                {
                    Message = $"Leave request with ID {id} not found.",
                    Data = new LeaveRequestResponseDto(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Message = "Leave request retrieved successfully.",
                Data = request,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("user/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRequestsByUserAsync(int userId)
        {
            var nameIdentifierClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(nameIdentifierClaim) || !int.TryParse(nameIdentifierClaim, out var currentUserId))
            {
                return Unauthorized(new
                {
                    Message = "User ID is missing from the token.",
                    Data = new List<LeaveRequestResponseDto>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            // Check authorization unless user is admin
            if (!User.IsInRole("Admin"))
            {
                var canAccess = await _resourceAuthorizationService.CanManagerAccessUserDataAsync(currentUserId, userId);
                if (!canAccess)
                {
                    return Forbid();
                }
            }

            var requests = await _leaveRequestService.GetRequestsByUserAsync(userId);
            if (requests == null || !requests.Any())
            {
                return NotFound(new
                {
                    Message = $"No leave requests found for user with ID {userId}.",
                    Data = new List<LeaveRequestResponseDto>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Message = "Leave requests retrieved successfully.",
                Data = requests,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllRequestsAsync()
        {
            var requests = await _leaveRequestService.GetAllRequestsAsync();
            if (requests == null || !requests.Any())
            {
                return NotFound(new
                {
                    Message = "No leave requests found.",
                    Data = new List<LeaveRequestResponseDto>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Message = "Leave requests retrieved successfully.",
                Data = requests,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("queried/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllRequestsFilteredAsync(int id, [FromQuery] QueriedLeaveRequestRequestDto payload)
        {
            var requests = await _leaveRequestService.GetAllLeaveRequestsFilteredAsync(id, payload);
            if (requests == null || requests.Data == null || !requests.Data.Any())
            {
                return NotFound(new
                {
                    Message = "No filtered leave requests found.",
                    Data = new List<LeaveRequestResponseDto>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Message = "Filtered leave requests retrieved successfully.",
                Data = requests,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [Authorize(Roles = "Manager")]
        [HttpPatch("review/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ReviewLeaveRequestAsync(int id, [FromBody] ReviewLeaveRequestDto dto)
        {
            var nameIdentifierClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(nameIdentifierClaim))
            {
                return Unauthorized(new
                {
                    Message = "Manager ID is missing from the token.",
                    Data = new List<LeaveRequestResponseDto>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            if (!int.TryParse(nameIdentifierClaim, out var managerId))
            {
                return BadRequest(new
                {
                    Message = "Invalid Manager ID.",
                    Data = new List<LeaveRequestResponseDto>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            await _leaveRequestService.ReviewLeaveRequestAsync(id, dto, managerId);
            return Ok(new
            {
                Message = "Leave request reviewed successfully.",
                Data = new List<LeaveRequestResponseDto>(),
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateLeaveRequestAsync(int id, [FromBody] UpdateLeaveRequestDto dto)
        {
            var result = await _leaveRequestService.UpdateLeaveRequestAsync(id, dto);
            return Ok(new
            {
                Message = "Leave request updated successfully.",
                Data = result,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [Authorize(Roles = "Employee")]
        [HttpPatch("by-employee/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CancelLeaveRequestAsync(int id)
        {
            var nameIdentifierClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(nameIdentifierClaim))
            {
                return Unauthorized(new
                {
                    Message = "User ID is missing from the token.",
                    Data = new List<LeaveRequestResponseDto>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            if (!int.TryParse(nameIdentifierClaim, out var userId))
            {
                return BadRequest(new
                {
                    Message = "Invalid User ID.",
                    Data = new List<LeaveRequestResponseDto>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            await _leaveRequestService.CancelLeaveRequestAsync(id, userId);

            return Ok(new { Message = "Leave request canceled successfully." });
        }


        [Authorize(Roles = "Manager")]
        [HttpGet("by-manager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRequestsByManager([FromQuery] string? name = null)
        {
            var nameIdentifierClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(nameIdentifierClaim))
            {
                return Unauthorized(new
                {
                    Message = "Manager ID is missing from the token.",
                    Data = new List<LeaveRequestResponseDto>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            if (!int.TryParse(nameIdentifierClaim, out var managerId))
            {
                return BadRequest(new
                {
                    Message = "Invalid Manager ID.",
                    Data = new List<LeaveRequestResponseDto>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            var requests = await _leaveRequestService.GetLeaveRequestsForManagerAsync(managerId, name);
            if (requests == null || !requests.Any())
            {
                return NotFound(new
                {
                    Message = "No leave requests found for this manager.",
                    Data = new List<LeaveRequestResponseDto>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Message = "Manager's leave requests retrieved successfully.",
                Data = requests,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [Authorize(Roles = "Employee")]
        [HttpGet("by-employee")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRequestsByEmployeeAsync()
        {
            var nameIdentifierClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(nameIdentifierClaim))
            {
                return Unauthorized(new
                {
                    Message = "User ID is missing from the token.",
                    Data = new List<LeaveRequestResponseDto>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            if (!int.TryParse(nameIdentifierClaim, out var userId))
            {
                return BadRequest(new
                {
                    Message = "Invalid User ID.",
                    Data = new List<LeaveRequestResponseDto>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            var requests = await _leaveRequestService.GetRequestsByUserAsync(userId);
            if (requests == null || !requests.Any())
            {
                return NotFound(new
                {
                    Message = "No leave requests found for this user.",
                    Data = new List<LeaveRequestResponseDto>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Message = "Leave requests retrieved successfully.",
                Data = requests,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [Authorize(Roles = "Manager")]
        [HttpGet("remaining-days/{userId}/{leaveRequestTypeId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRemainingLeaveDaysAsync(int userId, int leaveRequestTypeId, [FromQuery] int? year = null)
        {
            try
            {
                var currentYear = year ?? DateTime.Now.Year;
                var remainingDays = await _leaveRequestService.GetRemainingLeaveDaysAsync(userId, leaveRequestTypeId, currentYear);
                
                return Ok(new
                {
                    Message = "Remaining leave days retrieved successfully.",
                    Data = remainingDays,
                    Success = true,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (EntryNotFoundException ex)
            {
                return NotFound(new
                {
                    Message = ex.Message,
                    Data = new object(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving remaining leave days");
                return StatusCode(500, new
                {
                    Message = "An error occurred while retrieving remaining leave days.",
                    Data = new object(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        [Authorize(Roles = "Manager")]
        [HttpGet("remaining-days-for-period/{userId}/{leaveRequestTypeId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRemainingLeaveDaysForPeriodAsync(int userId, int leaveRequestTypeId, [FromQuery] string startDate, [FromQuery] string endDate)
        {
            try
            {
                if (!DateTime.TryParse(startDate, out DateTime parsedStartDate))
                {
                    return BadRequest(new
                    {
                        Message = "Invalid start date format. Please use YYYY-MM-DD format.",
                        Data = new object(),
                        Success = false,
                        Timestamp = DateTime.UtcNow
                    });
                }

                if (!DateTime.TryParse(endDate, out DateTime parsedEndDate))
                {
                    return BadRequest(new
                    {
                        Message = "Invalid end date format. Please use YYYY-MM-DD format.",
                        Data = new object(),
                        Success = false,
                        Timestamp = DateTime.UtcNow
                    });
                }

                if (parsedEndDate < parsedStartDate)
                {
                    return BadRequest(new
                    {
                        Message = "End date cannot be before start date.",
                        Data = new object(),
                        Success = false,
                        Timestamp = DateTime.UtcNow
                    });
                }

                var remainingDays = await _leaveRequestService.GetRemainingLeaveDaysForPeriodAsync(userId, leaveRequestTypeId, parsedStartDate, parsedEndDate);
                
                return Ok(new
                {
                    Message = "Remaining leave days for period calculated successfully.",
                    Data = remainingDays,
                    Success = true,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (EntryNotFoundException ex)
            {
                return NotFound(new
                {
                    Message = ex.Message,
                    Data = new object(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while calculating remaining leave days for period");
                return StatusCode(500, new
                {
                    Message = "An error occurred while calculating remaining leave days for the specified period.",
                    Data = new object(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        [Authorize(Roles = "Employee")]
        [HttpGet("by-employee/remaining-days/{leaveRequestTypeId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRemainingLeaveDaysForEmployeeAsync(int leaveRequestTypeId, [FromQuery] int? year = null)
        {
            var nameIdentifierClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(nameIdentifierClaim))
            {
                return Unauthorized(new
                {
                    Message = "User ID is missing from the token.",
                    Data = new object(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            if (!int.TryParse(nameIdentifierClaim, out var userId))
            {
                return BadRequest(new
                {
                    Message = "Invalid User ID.",
                    Data = new object(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            return await GetRemainingLeaveDaysAsync(userId, leaveRequestTypeId, year);
        }

        [Authorize(Roles = "Employee")]
        [HttpGet("by-employee/remaining-days-for-period/{leaveRequestTypeId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRemainingLeaveDaysForPeriodForEmployeeAsync(int leaveRequestTypeId, [FromQuery] string startDate, [FromQuery] string endDate)
        {
            var nameIdentifierClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(nameIdentifierClaim))
            {
                return Unauthorized(new
                {
                    Message = "User ID is missing from the token.",
                    Data = new object(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            if (!int.TryParse(nameIdentifierClaim, out var userId))
            {
                return BadRequest(new
                {
                    Message = "Invalid User ID.",
                    Data = new object(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            return await GetRemainingLeaveDaysForPeriodAsync(userId, leaveRequestTypeId, startDate, endDate);
        }

       
        [Authorize(Roles = "Manager")]
        [HttpGet("filtered")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFilteredLeaveRequestsAsync(
            [FromQuery] string? status = "ALL",
            [FromQuery] int pageSize = 10,
            [FromQuery] int pageNumber = 1)
        {
            if (pageSize <= 0)
                return BadRequest(new { Message = "Page size must be greater than 0" });
            if (pageNumber <= 0)
                return BadRequest(new { Message = "Page number must be greater than 0" });

            var nameIdentifierClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(nameIdentifierClaim))
            {
                return Unauthorized(new
                {
                    Message = "Manager ID is missing from the token.",
                    Data = new List<LeaveRequestResponseDto>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            if (!int.TryParse(nameIdentifierClaim, out var managerId))
            {
                return BadRequest(new
                {
                    Message = "Invalid Manager ID.",
                    Data = new List<LeaveRequestResponseDto>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            var (items, totalCount) = await _leaveRequestService.GetFilteredLeaveRequestsAsync(
                managerId,
                status ?? "ALL",
                pageSize,
                pageNumber);
            
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return Ok(new
            {
                Message = "Leave requests retrieved successfully.",
                Data = new
                {
                    Items = items,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    HasNextPage = pageNumber < totalPages,
                    HasPreviousPage = pageNumber > 1
                },
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}