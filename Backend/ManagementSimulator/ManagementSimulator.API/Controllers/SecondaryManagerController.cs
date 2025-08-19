using ManagementSimulator.Core.Dtos.Requests.SecondaryManagers;
using ManagementSimulator.Core.Dtos.Responses.SecondaryManager;
using ManagementSimulator.Core.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ManagementSimulator.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SecondaryManagerController : ControllerBase
    {
        private readonly ILogger<SecondaryManagerController> _logger;
        private readonly ISecondaryManagerService _secondaryManagerService;

        public SecondaryManagerController(ISecondaryManagerService secondaryManagerService, ILogger<SecondaryManagerController> logger)
        {
            _secondaryManagerService = secondaryManagerService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AssignSecondaryManagerAsync([FromBody] CreateSecondaryManagerRequest request)
        {
            var nameIdentifierClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(nameIdentifierClaim) || !int.TryParse(nameIdentifierClaim, out var adminId))
            {
                return Unauthorized(new
                {
                    Message = "Admin ID is missing or invalid in the token.",
                    Data = new SecondaryManagerResponseDto(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            var result = await _secondaryManagerService.AssignSecondaryManagerAsync(request, adminId);

            return Created($"/api/secondarymanager/", new
            {
                Message = "Secondary manager assigned successfully.",
                Data = result,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpPut("{employeeId}/{secondaryManagerId}/{startDate}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateSecondaryManagerAsync(int employeeId, int secondaryManagerId, DateTime startDate, [FromBody] UpdateSecondaryManagerRequest request)
        {
            var result = await _secondaryManagerService.UpdateSecondaryManagerAsync(employeeId, secondaryManagerId, startDate, request);

            return Ok(new
            {
                Message = "Secondary manager updated successfully.",
                Data = result,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpDelete("{employeeId}/{secondaryManagerId}/{startDate}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveSecondaryManagerAsync(int employeeId, int secondaryManagerId, DateTime startDate)
        {
            await _secondaryManagerService.RemoveSecondaryManagerAsync(employeeId, secondaryManagerId, startDate);

            return Ok(new
            {
                Message = "Secondary manager removed successfully.",
                Data = new SecondaryManagerResponseDto(),
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllSecondaryManagersAsync()
        {
            var secondaryManagers = await _secondaryManagerService.GetAllSecondaryManagersAsync();

            if (secondaryManagers == null || !secondaryManagers.Any())
            {
                return NotFound(new
                {
                    Message = "No secondary manager assignments found.",
                    Data = new List<SecondaryManagerResponseDto>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Message = "Secondary manager assignments retrieved successfully.",
                Data = secondaryManagers,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("employee/{employeeId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSecondaryManagersForEmployeeAsync(int employeeId)
        {
            var secondaryManagers = await _secondaryManagerService.GetSecondaryManagersForEmployeeAsync(employeeId);

            if (secondaryManagers == null || !secondaryManagers.Any())
            {
                return NotFound(new
                {
                    Message = $"No secondary managers found for employee with Id {employeeId}.",
                    Data = new List<SecondaryManagerResponseDto>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Message = "Secondary managers for employee retrieved successfully.",
                Data = secondaryManagers,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("employee/{employeeId}/active")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetActiveSecondaryManagersForEmployeeAsync(int employeeId)
        {
            var activeSecondaryManagers = await _secondaryManagerService.GetActiveSecondaryManagersForEmployeeAsync(employeeId);

            return Ok(new
            {
                Message = "Active secondary managers for employee retrieved successfully.",
                Data = activeSecondaryManagers,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("manager/{secondaryManagerId}/employees")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetEmployeesWithActiveSecondaryManagerAsync(int secondaryManagerId)
        {
            var employees = await _secondaryManagerService.GetEmployeesWithActiveSecondaryManagerAsync(secondaryManagerId);

            if (employees == null || !employees.Any())
            {
                return NotFound(new
                {
                    Message = $"No employees found with active secondary manager Id {secondaryManagerId}.",
                    Data = new List<SecondaryManagerResponseDto>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Message = "Employees with active secondary manager retrieved successfully.",
                Data = employees,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("assigned-by-admin")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSecondaryManagersAssignedByCurrentAdminAsync()
        {
            var nameIdentifierClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(nameIdentifierClaim) || !int.TryParse(nameIdentifierClaim, out var adminId))
            {
                return BadRequest(new
                {
                    Message = "Admin ID is missing or invalid in the token.",
                    Data = new List<SecondaryManagerResponseDto>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            var secondaryManagers = await _secondaryManagerService.GetSecondaryManagersAssignedByAdminAsync(adminId);

            return Ok(new
            {
                Message = "Secondary managers assigned by current admin retrieved successfully.",
                Data = secondaryManagers,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("{employeeId}/{secondaryManagerId}/{startDate}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSecondaryManagerByIdAsync(int employeeId, int secondaryManagerId, DateTime startDate)
        {
            var secondaryManager = await _secondaryManagerService.GetSecondaryManagerByIdAsync(employeeId, secondaryManagerId, startDate);

            if (secondaryManager == null)
            {
                return NotFound(new
                {
                    Message = $"Secondary manager assignment not found for employee {employeeId}, manager {secondaryManagerId}, start date {startDate}.",
                    Data = new SecondaryManagerResponseDto(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Message = "Secondary manager assignment retrieved successfully.",
                Data = secondaryManager,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("expired")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetExpiredSecondaryManagersAsync()
        {
            var expiredManagers = await _secondaryManagerService.GetExpiredSecondaryManagersAsync();

            return Ok(new
            {
                Message = "Expired secondary manager assignments retrieved successfully.",
                Data = expiredManagers,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("validate")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ValidateSecondaryManagerAssignmentAsync([FromQuery] int employeeId, [FromQuery] int secondaryManagerId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var isValid = await _secondaryManagerService.ValidateSecondaryManagerAssignmentAsync(employeeId, secondaryManagerId, startDate, endDate);

            return Ok(new
            {
                Message = isValid ? "Assignment is valid." : "Assignment is not valid.",
                Data = new { IsValid = isValid },
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}