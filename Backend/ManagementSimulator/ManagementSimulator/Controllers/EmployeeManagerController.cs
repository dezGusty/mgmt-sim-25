using Azure.Core;
using ManagementSimulator.Core.Dtos.Requests.EmployeeManagers;
using ManagementSimulator.Core.Dtos.Requests.UserManagers;
using ManagementSimulator.Core.Dtos.Responses;
using ManagementSimulator.Core.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ManagementSimulator.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeManagerController : ControllerBase
    {
        private readonly ILogger<EmployeeManagerController> _logger;
        private readonly IEmployeeManagerService _employeeManagerService;

        public EmployeeManagerController(IEmployeeManagerService employeeManagerService, ILogger<EmployeeManagerController> logger)
        {
            _employeeManagerService = employeeManagerService;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddEmployeeManagerAsync([FromBody] CreateEmployeeManagerRequest request)
        {
            await _employeeManagerService.AddEmployeeManagerAsync(request.EmployeeId, request.ManagerId);

            return Created($"/api/employeemanager/", new
            {
                Message = "Employee-Manager relationship created successfully.",
                Data = new EmployeeManagerResponseDto(),
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpPost("addManagersForEmployee")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddManagersForEmployeeAsync([FromBody] CreateManagersForEmployeeRequest payload)
        {
            await _employeeManagerService.AddManagersForEmployeeAsync(payload.EmployeeId, payload.ManagersIds);
            return Created($"/api/employeemanager/addManagersForEmployee", new
            {
                Message = "Employee-Manager relationships created successfully.",
                Data = new EmployeeManagerResponseDto(),
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }


        [HttpPatch("patchManagersForEmployee")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PatchManagersForEmployeeAsync([FromBody] CreateManagersForEmployeeRequest payload)
        {
            await _employeeManagerService.PatchManagersForEmployeeAsync(payload.EmployeeId, payload.ManagersIds);
            return Created($"/api/employeemanager/addManagersForEmployee", new
            {
                Message = "Employee-Manager relationships created successfully.",
                Data = new EmployeeManagerResponseDto(),
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllEmployeeManagersAsync()
        {
            var employeeManagers = await _employeeManagerService.GetAllEmployeeManagersAsync();

            if (employeeManagers == null || !employeeManagers.Any())
            {
                return NotFound(new
                {
                    Message = "No employee-manager relationships found.",
                    Data = new List<EmployeeManagerResponseDto>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Message = "Employee-Manager relationships retrieved successfully.",
                Data = employeeManagers,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("managers/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetManagersForEmployee(int id)
        {
            var employeeManagers = await _employeeManagerService.GetManagersByEmployeeIdAsync(id);

            if (employeeManagers == null || !employeeManagers.Any())
            {
                return NotFound(new
                {
                    Message = $"No managers found for employee with Id {id}.",
                    Data = new List<EmployeeManagerResponseDto>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Message = "Managers for employee retrieved successfully.",
                Data = employeeManagers,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("employees/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetEmployeesForManagers(int id)
        {
            var employeeManagers = await _employeeManagerService.GetEmployeesByManagerIdAsync(id);

            if (employeeManagers == null || !employeeManagers.Any())
            {
                return NotFound(new
                {
                    Message = $"No employees found for manager with Id {id}.",
                    Data = new List<EmployeeManagerResponseDto>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Message = "Employees for manager retrieved successfully.",
                Data = employeeManagers,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [Authorize(Roles = "Manager")]
        [HttpGet("employeesByManager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetEmployeesForManagers()
        {
            var nameIdentifierClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(nameIdentifierClaim))
            {
                return Unauthorized(new
                {
                    Message = "Manager ID is missing from the token.",
                    Data = new EmployeeManagerResponseDto(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            if (!int.TryParse(nameIdentifierClaim, out var managerId))
            {
                return BadRequest(new
                {
                    Message = "Invalid Manager ID format.",
                    Data = new EmployeeManagerResponseDto(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            var employeeManagers = await _employeeManagerService.GetEmployeesByManagerIdAsync(managerId);

            if (employeeManagers == null || !employeeManagers.Any())
            {
                return NotFound(new
                {
                    Message = $"No employees found for manager with Id {managerId}.",
                    Data = new List<EmployeeManagerResponseDto>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Message = "Employees for current manager retrieved successfully.",
                Data = employeeManagers,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpDelete("{employeeId}/{managerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteEmployeeManagerAsync(int employeeId, int managerId)
        {
            await _employeeManagerService.DeleteEmployeeManagerAsync(employeeId, managerId);

            return Ok(new
            {
                Message = "Employee-Manager relationship deleted successfully.",
                Data = new EmployeeManagerResponseDto(),
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpPatch("employee/{employeeId}/{managerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateEmployeeForManagerAsync(int employeeId, int managerId, [FromBody] UpdateEmployeeForManagerRequest request)
        {
            var updatedRelationship = await _employeeManagerService.UpdateEmployeeForManagerAsync(employeeId, managerId, request.NewEmployeeId);

            return Ok(new
            {
                Message = "Employee updated for manager successfully.",
                Data = updatedRelationship,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpPatch("manager/{employeeId}/{managerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateManagerForEmployeeAsync(int employeeId, int managerId, [FromBody] UpdateManagerForEmployeeRequest request)
        {
            var updatedRelationship = await _employeeManagerService.UpdateManagerForEmployeeAsync(employeeId, managerId, request.NewManagerId);

            return Ok(new
            {
                Message = "Manager updated for employee successfully.",
                Data = updatedRelationship,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}