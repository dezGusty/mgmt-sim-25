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
            try
            {
                var types = await _leaveRequestTypeService.GetAllAsync();
                if (types == null || !types.Any())
                {
                    return NotFound("No leave request types found.");
                }
                return Ok(types);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving leave request types");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving leave request types.");
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            try
            {
                var type = await _leaveRequestTypeService.GetByIdAsync(id);
                if (type == null)
                {
                    return NotFound($"Leave request type with ID {id} not found.");
                }
                return Ok(type);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving leave request type by ID");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the leave request type.");
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddAsync([FromBody] CreateLeaveRequestTypeRequestDto dto)
        {
            try
            {
                var result = await _leaveRequestTypeService.AddAsync(dto);

                return Created($"/api/LeaveRequestType/{result.Id}", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating leave request type");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the leave request type.");
            }
        }




        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateLeaveRequestTypeRequestDto dto)
        {
            try
            {
                var updatedType = await _leaveRequestTypeService.UpdateAsync(id, dto);
                if (updatedType == null)
                {
                    return NotFound($"Leave request type with ID {id} not found.");
                }
                return Ok(updatedType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating leave request type");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the leave request type.");
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            try
            {
                bool result = await _leaveRequestTypeService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound($"Leave request type with ID {id} not found.");
                }
                return Ok($"Leave request type with ID {id} deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting leave request type");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the leave request type.");
            }
        }
    }
}