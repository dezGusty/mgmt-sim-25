using ManagementSimulator.Core.Dtos.Requests.LeaveRequestType;
using ManagementSimulator.Core.Dtos.Requests.LeaveRequestTypes;
using ManagementSimulator.Core.Dtos.Responses;
using ManagementSimulator.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ManagementSimulator.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaveRequestTypeController : ControllerBase
    {
        private readonly ILeaveRequestTypeService _leaveRequestTypeService;
        private readonly ILogger<LeaveRequestTypeController> _logger;

        public LeaveRequestTypeController(ILeaveRequestTypeService leaveRequestTypeService, ILogger<LeaveRequestTypeController> logger)
        {
            _leaveRequestTypeService = leaveRequestTypeService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllLeaveRequestTypesAsync()
        {
            var types = await _leaveRequestTypeService.GetAllLeaveRequestTypesAsync();
            if (types == null || !types.Any())
            {
                return NotFound(new
                {
                    Message = "No leave request types found.",
                    Data = new List<object>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }
            return Ok(new
            {
                Message = "Leave request types retrieved successfully.",
                Data = types,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("queried")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllLeaveRequestTypesFilteredAsync([FromQuery] QueriedLeaveRequestTypeRequestDto payload)
        {
            var types = await _leaveRequestTypeService.GetAllLeaveRequestTypesFilteredAsync(payload);
            if (types.Data == null || !types.Data.Any())
            {
                return NotFound(new
                {
                    Message = "No filtered leave request types found.",
                    Data = new List<object>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Message = "Filtered leave request types retrieved successfully.",
                Data = types,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var type = await _leaveRequestTypeService.GetLeaveRequestTypeByIdAsync(id);
            if (type == null)
            {
                return NotFound(new
                {
                    Message = $"Leave request type with ID {id} not found.",
                    Data = new List<LeaveRequestTypeResponseDto>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Message = "Leave request type retrieved successfully.",
                Data = type,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddAsync([FromBody] CreateLeaveRequestTypeRequestDto dto)
        {
            var result = await _leaveRequestTypeService.AddLeaveRequestTypeAsync(dto);
            return Created($"/api/LeaveRequestType/{result.Id}", new
            {
                Message = "Leave request type created successfully.",
                Data = result,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateLeaveRequestTypeRequestDto dto)
        {
            var updatedType = await _leaveRequestTypeService.UpdateLeaveRequestTypeAsync(id, dto);
            if (updatedType == null)
            {
                return NotFound(new
                {
                    Message = $"Leave request type with ID {id} not found.",
                    Data = new List<LeaveRequestTypeResponseDto>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Message = "Leave request type updated successfully.",
                Data = updatedType,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            bool result = await _leaveRequestTypeService.DeleteLeaveRequestTypeAsync(id);
            if (!result)
            {
                return NotFound(new
                {
                    Message = $"Leave request type with ID {id} not found.",
                    Data = new List<LeaveRequestTypeResponseDto>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Message = $"Leave request type with ID {id} deleted successfully.",
                Data = result,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}