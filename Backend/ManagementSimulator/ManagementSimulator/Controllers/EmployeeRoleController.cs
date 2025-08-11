using ManagementSimulator.Core.Dtos.Responses;
using ManagementSimulator.Core.Services;
using ManagementSimulator.Core.Services.Interfaces;
using ManagementSimulator.Database.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ManagementSimulator.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmployeeRoleController: ControllerBase
    {
        private readonly IEmployeeRoleService _employeeRoleService;
        private readonly ILogger<EmployeeRole> _logger;

        public EmployeeRoleController(IEmployeeRoleService employeeRoleService,ILogger<EmployeeRole> logger) 
        {
            _employeeRoleService = employeeRoleService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllEmployeeRoles()
        {
            var result = await _employeeRoleService.GetAllEmployeeRolesAsync();

            if (result == null)
            {
                return NotFound(new
                {
                    Message = "No user roles found.",
                    Data = new List<EmployeeRoleResponseDto>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Message = "User roles successfully retrieved.",
                Data = result,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
