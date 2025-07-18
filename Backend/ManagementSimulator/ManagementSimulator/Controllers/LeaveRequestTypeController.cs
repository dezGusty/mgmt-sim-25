using ManagementSimulator.Core.Dtos.Requests.LeaveRequestType;
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
        public async Task<IActionResult> GetAllAsync()
        {
            var types = await _leaveRequestTypeService.GetAllLeaveRequestTypesAsync();
            if (types == null || !types.Any())
            {
                return NotFound("No leave request types found.");
            }
            return Ok(types);
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
                return NotFound($"Leave request type with ID {id} not found.");
            }
            return Ok(type);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddAsync([FromBody] CreateLeaveRequestTypeRequestDto dto)
        {
            var result = await _leaveRequestTypeService.AddLeaveRequestTypeAsync(dto);

            return Created($"/api/LeaveRequestType/{result.Id}", result);
        }

        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateLeaveRequestTypeRequestDto dto)
        {
            var updatedType = await _leaveRequestTypeService.UpdateLeaveRequestTypeAsync(id, dto);
            if (updatedType == null)
            {
                return NotFound($"Leave request type with ID {id} not found.");
            }
            return Ok(updatedType);
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
                return NotFound($"Leave request type with ID {id} not found.");
            }
            return Ok($"Leave request type with ID {id} deleted successfully.");
        }
    }
}