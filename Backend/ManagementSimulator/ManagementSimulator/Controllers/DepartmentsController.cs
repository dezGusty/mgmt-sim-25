using ManagementSimulator.Core.Dtos.Requests.Departments;
using ManagementSimulator.Core.Dtos.Responses;
using ManagementSimulator.Core.Services.Interfaces;
using ManagementSimulator.Database.Entities;
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
                return NotFound(new 
                {
                    Message = "No departments found.",
                    Data = new List<DepartmentResponseDto>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Message = "Departmens retrieved succesfully.",
                Data = departments,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
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
                return NotFound(new
                {
                    Message = "No departments found.",
                    Data = new List<DepartmentResponseDto>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Message = "Departmens retrieved succesfully.",
                Data = departments,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
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
                return NotFound(new
                {
                    Message = "No departments found.",
                    Data = new DepartmentResponseDto(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Message = "Departmens retrieved succesfully.",
                Data = department,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddDepartmentAsync([FromBody] CreateDepartmentRequestDto dto)
        {
            var department = await _departmentService.AddDepartmentAsync(dto);
            return Created($"/api/departments/{department.Id}", new
            {
                Message = "Department created successfully.",
                Data = department,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
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
                return NotFound(new
                {
                    Message = $"Department with Id {id} not found.",
                    Data = new DepartmentResponseDto(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Message = "Department created successfully.",
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
        public async Task<IActionResult> UpdateDepartmentAsync(int id, [FromBody] UpdateDepartmentRequestDto dto)
        {
            var updatedDepartment = await _departmentService.UpdateDepartmentAsync(id, dto);
            if (updatedDepartment == null)
            {
                return NotFound(new
                {
                    Message = $"Department with Id {id} not found.",
                    Data = new DepartmentResponseDto(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Message = "Department created successfully.",
                Data = updatedDepartment,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}