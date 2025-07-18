using ManagementSimulator.Core.Dtos.Requests.LeaveRequest;
using ManagementSimulator.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> AddLeaveRequestAsync([FromBody] LeaveRequestRequestDto dto)
        {
            try
            {
                var newRequestId = await _leaveRequestService.AddLeaveRequestAsync(dto);
                return Created($"/api/leaveRequests/{newRequestId}", newRequestId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating leave request.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the leave request.");
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLeaveRequestByIdAsync(int id)
        {
            try
            {
                var request = await _leaveRequestService.GetRequestByIdAsync(id);
                if (request == null)
                {
                    return NotFound($"Leave request with ID {id} not found.");
                }
                return Ok(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving leave request.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the leave request.");
            }
        }

        [HttpGet("user/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRequestsByUserAsync(int userId)
        {
            try
            {
                var requests = await _leaveRequestService.GetRequestsByUserAsync(userId);
                if (!requests.Any())
                {
                    return NotFound($"No leave requests found for user with ID {userId}.");
                }
                return Ok(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving leave requests for user.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving leave requests.");
            }
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllRequestsAsync()
        {
            try
            {
                var requests = await _leaveRequestService.GetAllRequestsAsync();
                return Ok(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all leave requests.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving leave requests.");
            }
        }


        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ReviewLeaveRequestAsync(int id, [FromBody] ReviewLeaveRequestDto dto)
        {
            try
            {
                await _leaveRequestService.ReviewLeaveRequestAsync(id, dto);
                return Ok("Leave request reviewed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error reviewing leave request with ID {id}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while reviewing the leave request.");
            }
        }
    }
}
