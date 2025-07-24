using Azure.Core;
using ManagementSimulator.Core.Dtos.Requests.LeaveRequest;
using ManagementSimulator.Core.Dtos.Requests.LeaveRequests;
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

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddLeaveRequestAsync([FromBody] CreateLeaveRequestRequestDto dto)
        {
            var newRequestId = await _leaveRequestService.AddLeaveRequestAsync(dto);
            return Created($"/api/leaveRequests/{newRequestId}", newRequestId);
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
                return NotFound($"Leave request with ID {id} not found.");
            }
            return Ok(request);
        }

        [HttpGet("user/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRequestsByUserAsync(int userId)
        {
            var requests = await _leaveRequestService.GetRequestsByUserAsync(userId);
            if (!requests.Any())
            {
                return NotFound($"No leave requests found for user with ID {userId}.");
            }
            return Ok(requests);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllRequestsAsync()
        {
            var requests = await _leaveRequestService.GetAllRequestsAsync();
            return Ok(requests);
        }

        [Authorize(Roles = "Manager")]
        [HttpPatch("review/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ReviewLeaveRequestAsync(int id, [FromBody] ReviewLeaveRequestDto dto)
        {
            var nameIdentifierClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(nameIdentifierClaim))
            {
                return Unauthorized("Manager ID is missing from the token.");
            }

            if (!int.TryParse(nameIdentifierClaim, out var managerId))
            {
                return BadRequest("Invalid Manager ID.");
            }

            await _leaveRequestService.ReviewLeaveRequestAsync(id, dto, managerId);
            return Ok(new { message = "Leave request reviewed successfully." });
        }

        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateLeaveRequestAsync(int id, [FromBody] UpdateLeaveRequestDto dto)
        {
            await _leaveRequestService.UpdateLeaveRequestAsync(id, dto);
            return Ok("Leave request reviewed successfully.");
        }

        [Authorize(Roles = "Manager")]
        [HttpGet("by-manager")]
        public async Task<IActionResult> GetRequestsByManager()
        {
            var nameIdentifierClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(nameIdentifierClaim))
            {
                return Unauthorized("Manager ID is missing from the token.");
            }

            if (!int.TryParse(nameIdentifierClaim, out var managerId))
            {
                return BadRequest("Invalid Manager ID.");
            }

            var requests = await _leaveRequestService.GetLeaveRequestsForManagerAsync(managerId);
            return Ok(requests);
        }
    }
}
