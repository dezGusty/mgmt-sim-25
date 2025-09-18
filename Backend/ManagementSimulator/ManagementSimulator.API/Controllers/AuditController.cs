using ManagementSimulator.Core.Services.Interfaces;
using ManagementSimulator.Database.Repositories.Intefaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManagementSimulator.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AuditController : ControllerBase
    {
        private readonly IAuditService _auditService;
        private readonly IUserService _userService;
        private readonly ILogger<AuditController> _logger;

        public AuditController(IAuditService auditService, IUserService userService, ILogger<AuditController> logger)
        {
            _auditService = auditService;
            _userService = userService;
            _logger = logger;
        }

        [HttpGet("user/{userId}/actions")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetUserAuditActions(int userId)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new
                    {
                        Message = "User not found.",
                        Data = (object?)null,
                        Success = false,
                        Timestamp = DateTime.UtcNow
                    });
                }

                return Ok(new
                {
                    Message = "User audit information retrieved successfully.",
                    Data = new
                    {
                        UserId = userId,
                        UserDisplayName = user.Email,
                        Note = "Full audit trail requires database queries for specific entities."
                    },
                    Success = true,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit information for user {UserId}", userId);
                return StatusCode(500, new
                {
                    Message = "An error occurred while retrieving audit information.",
                    Data = (object?)null,
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpGet("current-user")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCurrentUserAuditInfo()
        {
            try
            {
                var currentUserId = _auditService.GetCurrentUserId();
                if (currentUserId == null)
                {
                    return Unauthorized(new
                    {
                        Message = "User is not authenticated.",
                        Data = (object?)null,
                        Success = false,
                        Timestamp = DateTime.UtcNow
                    });
                }

                var user = await _userService.GetUserByIdAsync(currentUserId.Value);
                var userDisplayName = user?.Email ?? $"User #{currentUserId.Value}";

                return Ok(new
                {
                    Message = "Current user audit information retrieved successfully.",
                    Data = new
                    {
                        UserId = currentUserId.Value,
                        UserDisplayName = userDisplayName,
                        Note = "This user ID will be used for audit logging of actions."
                    },
                    Success = true,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current user audit information");
                return StatusCode(500, new
                {
                    Message = "An error occurred while retrieving audit information.",
                    Data = (object?)null,
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }
        }
    }
}