using Azure.Core;
using ManagementSimulator.Core.Dtos.Requests.LeaveRequest;
using ManagementSimulator.Core.Dtos.Requests.LeaveRequests;
using ManagementSimulator.Core.Dtos.Responses.LeaveRequest;
using ManagementSimulator.Core.Services;
using ManagementSimulator.Core.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ManagementSimulator.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaveRequestsController : ControllerBase
    {
        private readonly ILeaveRequestService _leaveRequestService;
        private readonly ILogger<LeaveRequestsController> _logger;

        public LeaveRequestsController(ILeaveRequestService leaveRequestService, ILogger<LeaveRequestsController> logger)
        {
            _leaveRequestService = leaveRequestService;
            _logger = logger;
        }
        
        [Authorize(Roles = "Manager")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddLeaveRequestAsync([FromBody] CreateLeaveRequestRequestDto dto)
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

        [Authorize(Roles = "Employee")]
        [HttpPost("by-employee")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

            var leaveRequest = await _leaveRequestService.AddLeaveRequestByEmployeeAsync(dto, userId);

            return Created($"api/LeaveRequests/{leaveRequest.Id}", new
            {
                Message = "Leave request created successfully.",
                Data = leaveRequest,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }


        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLeaveRequestByIdAsync(int id)
        {
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

        [HttpGet("user/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRequestsByUserAsync(int userId)
        {
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

        [Authorize(Roles = "Manager")]
        [HttpGet("by-manager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRequestsByManager()
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

            var requests = await _leaveRequestService.GetLeaveRequestsForManagerAsync(managerId);
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
    }
}