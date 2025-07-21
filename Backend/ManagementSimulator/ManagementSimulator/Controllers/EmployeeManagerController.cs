using ManagementSimulator.Core.Dtos.Requests.EmployeeManagers;
using ManagementSimulator.Core.Dtos.Requests.UserManagers;
using ManagementSimulator.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddEmployeeManagerAsync([FromBody] CreateEmployeeManagerRequest request)
        {
            await _employeeManagerService.AddEmployeeManagerAsync(request.EmployeeId, request.ManagerId);
            return Ok();
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

        [HttpGet("/employees/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetEmployeesForManagers(int id)
        {
            var employeeManagers = await _employeeManagerService.GetEmployeesByManagerIdAsync(id);
            return Ok(employeeManagers);
        }

        [HttpDelete("{employeeId}/{managerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteEmployeeManagerAsync(int employeeId, int managerId)
        {
            await _employeeManagerService.DeleteEmployeeManagerAsync(employeeId, managerId);
            return Ok();
        }


        [HttpPatch("/employee/{employeeId}/{managerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateEmployeeForManagerAsync(int employeeId, int managerId, [FromBody] UpdateEmployeeForManagerRequest request)
        {
            await _employeeManagerService.UpdateEmployeeForManagerAsync(employeeId, managerId, request.NewEmployeeId);
            return Ok();
        }


        [HttpPatch("/manager/{employeeId}/{managerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateManagerForEmployeeAsync(int employeeId, int managerId, [FromBody] UpdateManagerForEmployeeRequest request)
        {
            await _employeeManagerService.UpdateManagerForEmployeeAsync(employeeId, managerId, request.NewManagerId);
            return Ok();
        }
    }
}
