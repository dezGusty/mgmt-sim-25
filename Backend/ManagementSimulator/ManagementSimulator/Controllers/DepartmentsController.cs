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
            var departments = await _departmentService.GetAllDepartmentsAsync();
            if (departments == null || !departments.Any())
            {
                return NotFound(new { message = "No departments found." });
            }
            return Ok(departments);
        }

        [HttpGet("queried")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllDepartmentsFilteredAsync([FromQuery] QueriedDepartmentRequestDto payload)
        {
            var departments = await _departmentService.GetAllDepartmentsFilteredAsync(payload);
            if (departments == null || departments.Data == null || !departments.Data.Any())
            {
                return NotFound(new { message = "No departments found." });
            }
            return Ok(departments);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDepartmentByIdAsync(int id)
        {
            var department = await _departmentService.GetDepartmentByIdAsync(id);
            if (department == null)
            {
                return NotFound(new { message = $"Department with ID {id} not found." });
            }
            return Ok(department);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddDepartmentAsync([FromBody] CreateDepartmentRequestDto dto)
        {
            var department = await _departmentService.AddDepartmentAsync(dto);
            return Created($"/api/departments/{department.Id}", department);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteDepartmentAsync(int id)
        {
            bool result = await _departmentService.DeleteDepartmentAsync(id);
            if (!result)
            {
                return NotFound(new { message = $"Department with ID {id} not found." });
            }
            return Ok(new { message = $"Department with ID {id} deleted successfully." });
        }

        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateDepartmentAsync(int id, [FromBody] UpdateDepartmentRequestDto dto)
        {
            var updatedDepartment = await _departmentService.UpdateDepartmentAsync(id, dto);
            if (updatedDepartment == null)
            {
                return NotFound(new { message = $"Department with ID {id} not found." });
            }
            return Ok(updatedDepartment);
        }
    }
}