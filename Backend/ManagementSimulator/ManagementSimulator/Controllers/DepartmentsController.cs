using ManagementSimulator.Core.Dtos.Requests.Departments;
using ManagementSimulator.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ManagementSimulator.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;
        private readonly ILogger<DepartmentsController> _logger;

        public DepartmentsController(IDepartmentService departmentService, ILogger<DepartmentsController> logger)
        {
            _departmentService = departmentService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllDepartmentsAsync()
        {
            try
            {
                var departments = await _departmentService.GetAllDepartmentsAsync();
                if (departments == null || !departments.Any())
                {
                    return NotFound("No departments found.");
                }
                return Ok(departments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving departments");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving departments.");
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDepartmentByIdAsync(int id)
        {
            try
            {
                var department = await _departmentService.GetDepartmentByIdAsync(id);
                if (department == null)
                {
                    return NotFound($"Department with ID {id} not found.");
                }
                return Ok(department);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving department by ID");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the department.");
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddDepartmentAsync([FromBody] CreateDepartmentRequestDto dto)
        {
            try
            {
                var department = await _departmentService.AddDepartmentAsync(dto);
                return Ok(department);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the department.");
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteDepartmentAsync(int id)
        {
            try
            {
                bool result = await _departmentService.DeleteDepartmentAsync(id);
                if (!result)
                {
                    return NotFound($"Department with ID {id} not found.");
                }
                return Ok($"Department with ID {id} deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting department, department not found");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the department.");
            }
        }

        [HttpPatch]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateDepartmentAsync([FromBody] UpdateDepartmentRequestDto dto)
        {
            try
            {
                var updatedDepartment = await _departmentService.UpdateDepartmentAsync(dto);
                if (updatedDepartment == null)
                {
                    return NotFound($"Department with ID {dto.Id} not found.");
                }
                return Ok(updatedDepartment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating department");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the department.");
            }
        }
    }

}
