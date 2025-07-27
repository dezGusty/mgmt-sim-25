using ManagementSimulator.Core.Dtos.Requests.EmployeeManagers;
using ManagementSimulator.Core.Dtos.Requests.UserManagers;
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddEmployeeManagerAsync([FromBody] CreateEmployeeManagerRequest request)
        {
            await _employeeManagerService.AddEmployeeManagerAsync(request.EmployeeId, request.ManagerId);
            return Ok(new { message = "Employee-Manager relationship created successfully." });
        }

        [HttpGet()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllEmployeeManagersAsync()
        {
            var employeeManagers = await _employeeManagerService.GetAllEmployeeManagersAsync();
            return Ok(employeeManagers);
        }

        [HttpGet("/managers/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetManagersForEmployee(int id)
        {
            var employeeManagers = await _employeeManagerService.GetManagersByEmployeeIdAsync(id);
            return Ok(employeeManagers);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("/employees/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetEmployeesForManagers(int id)
        {
            var employeeManagers = await _employeeManagerService.GetEmployeesByManagerIdAsync(id);
            return Ok(employeeManagers);
        }

        [Authorize(Roles = "Manager")]
        [HttpGet("/employeesByManager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetEmployeesForManagers()
        {
            var nameIdentifierClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(nameIdentifierClaim))
                return Unauthorized(new { message = "Manager ID is missing from the token." });

            if (!int.TryParse(nameIdentifierClaim, out var managerId))
                return BadRequest(new { message = "Invalid Manager ID." });

            var employeeManagers = await _employeeManagerService.GetEmployeesByManagerIdAsync(managerId);
            return Ok(employeeManagers);
        }

        [HttpDelete("{employeeId}/{managerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteEmployeeManagerAsync(int employeeId, int managerId)
        {
            await _employeeManagerService.DeleteEmployeeManagerAsync(employeeId, managerId);
            return Ok(new { message = "Employee-Manager relationship deleted successfully." });
        }

        [HttpPatch("/employee/{employeeId}/{managerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateEmployeeForManagerAsync(int employeeId, int managerId, [FromBody] UpdateEmployeeForManagerRequest request)
        {
            await _employeeManagerService.UpdateEmployeeForManagerAsync(employeeId, managerId, request.NewEmployeeId);
            return Ok(new { message = "Employee updated for manager successfully." });
        }

        [HttpPatch("/manager/{employeeId}/{managerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateManagerForEmployeeAsync(int employeeId, int managerId, [FromBody] UpdateManagerForEmployeeRequest request)
        {
            await _employeeManagerService.UpdateManagerForEmployeeAsync(employeeId, managerId, request.NewManagerId);
            return Ok(new { message = "Manager updated for employee successfully." });
        }
    }
}