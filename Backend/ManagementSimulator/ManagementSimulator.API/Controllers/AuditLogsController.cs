using ManagementSimulator.Core.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ManagementSimulator.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AuditLogsController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<AuditLogsController> _logger;

        public AuditLogsController(IAuditLogService auditLogService, ILogger<AuditLogsController> logger)
        {
            _auditLogService = auditLogService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuditLogs(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string? userId = null,
            [FromQuery] string? action = null,
            [FromQuery] string? entityType = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? search = null)
        {
            try
            {
                if (page < 1)
                    return BadRequest(new { error = "Page must be greater than 0" });

                if (pageSize < 1 || pageSize > 100)
                    return BadRequest(new { error = "Page size must be between 1 and 100" });

                var result = await _auditLogService.GetAuditLogsAsync(
                    page,
                    pageSize,
                    userId,
                    action,
                    entityType,
                    startDate,
                    endDate,
                    search);

                return Ok(new
                {
                    message = "Audit logs retrieved successfully",
                    data = result,
                    success = true,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit logs");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("entity/{entityType}/{entityId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetEntityAuditTrail(string entityType, int entityId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(entityType))
                    return BadRequest(new { error = "Entity type is required" });

                if (entityId <= 0)
                    return BadRequest(new { error = "Invalid entity ID" });

                var auditTrail = await _auditLogService.GetEntityAuditTrailAsync(entityType, entityId);

                return Ok(new
                {
                    message = $"Audit trail for {entityType} {entityId} retrieved successfully",
                    data = auditTrail,
                    success = true,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit trail for {EntityType} {EntityId}", entityType, entityId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("user/{userId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserAuditTrail(
            int userId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                if (userId <= 0)
                    return BadRequest(new { error = "Invalid user ID" });

                var auditTrail = await _auditLogService.GetUserAuditTrailAsync(userId, startDate, endDate);

                return Ok(new
                {
                    message = $"Audit trail for user {userId} retrieved successfully",
                    data = auditTrail,
                    success = true,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit trail for user {UserId}", userId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("summary")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuditSummary([FromQuery] int days = 7)
        {
            try
            {
                if (days < 1 || days > 365)
                    return BadRequest(new { error = "Days must be between 1 and 365" });

                var endDate = DateTime.UtcNow;
                var startDate = endDate.AddDays(-days);

                var auditLogs = await _auditLogService.GetAuditLogsAsync(
                    page: 1,
                    pageSize: int.MaxValue,
                    startDate: startDate,
                    endDate: endDate);

                var summary = new
                {
                    TotalActions = auditLogs.TotalCount,
                    PeriodDays = days,
                    StartDate = startDate,
                    EndDate = endDate,
                    ActionBreakdown = auditLogs.Data
                        .GroupBy(a => a.Action)
                        .Select(g => new { Action = g.Key, Count = g.Count() })
                        .OrderByDescending(x => x.Count)
                        .ToList(),
                    EntityBreakdown = auditLogs.Data
                        .GroupBy(a => a.EntityType)
                        .Select(g => new { EntityType = g.Key, Count = g.Count() })
                        .OrderByDescending(x => x.Count)
                        .ToList(),
                    TopUsers = auditLogs.Data
                        .GroupBy(a => new { a.UserId, a.UserEmail })
                        .Select(g => new {
                            UserId = g.Key.UserId,
                            UserEmail = g.Key.UserEmail,
                            ActionCount = g.Count()
                        })
                        .OrderByDescending(x => x.ActionCount)
                        .Take(10)
                        .ToList(),
                    FailureRate = auditLogs.TotalCount > 0
                        ? Math.Round((double)auditLogs.Data.Count(a => !a.Success) / auditLogs.TotalCount * 100, 2)
                        : 0,
                    RecentFailures = auditLogs.Data
                        .Where(a => !a.Success)
                        .OrderByDescending(a => a.Timestamp)
                        .Take(5)
                        .Select(a => new {
                            a.Action,
                            a.EntityType,
                            a.UserEmail,
                            a.ErrorMessage,
                            a.Timestamp
                        })
                        .ToList()
                };

                return Ok(new
                {
                    message = "Audit summary retrieved successfully",
                    data = summary,
                    success = true,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit summary");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("actions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAvailableActions()
        {
            try
            {
                // Get distinct actions from recent audit logs to help with filtering
                var recentLogs = await _auditLogService.GetAuditLogsAsync(
                    page: 1,
                    pageSize: 1000,
                    startDate: DateTime.UtcNow.AddDays(-30));

                var actions = recentLogs.Data
                    .Select(a => a.Action)
                    .Distinct()
                    .OrderBy(a => a)
                    .ToList();

                var entityTypes = recentLogs.Data
                    .Select(a => a.EntityType)
                    .Distinct()
                    .OrderBy(e => e)
                    .ToList();

                return Ok(new
                {
                    message = "Available filter options retrieved successfully",
                    data = new
                    {
                        Actions = actions,
                        EntityTypes = entityTypes
                    },
                    success = true,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available actions");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("export")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ExportAuditLogs(
            [FromQuery] string format = "json",
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? userId = null,
            [FromQuery] string? action = null,
            [FromQuery] string? entityType = null)
        {
            try
            {
                if (!new[] { "json", "csv" }.Contains(format.ToLowerInvariant()))
                    return BadRequest(new { error = "Format must be 'json' or 'csv'" });

                // Limit export to reasonable size
                var maxExportSize = 10000;
                var auditLogs = await _auditLogService.GetAuditLogsAsync(
                    page: 1,
                    pageSize: maxExportSize,
                    userId: userId,
                    action: action,
                    entityType: entityType,
                    startDate: startDate,
                    endDate: endDate);

                if (format.ToLowerInvariant() == "json")
                {
                    return File(
                        System.Text.Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(auditLogs.Data, new System.Text.Json.JsonSerializerOptions { WriteIndented = true })),
                        "application/json",
                        $"audit-logs-{DateTime.UtcNow:yyyy-MM-dd-HH-mm-ss}.json");
                }
                else
                {
                    var csv = GenerateCsvFromAuditLogs(auditLogs.Data);
                    return File(
                        System.Text.Encoding.UTF8.GetBytes(csv),
                        "text/csv",
                        $"audit-logs-{DateTime.UtcNow:yyyy-MM-dd-HH-mm-ss}.csv");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting audit logs");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        private string GenerateCsvFromAuditLogs(IEnumerable<Database.Entities.AuditLog> auditLogs)
        {
            var csv = new System.Text.StringBuilder();

            // Header
            csv.AppendLine("Timestamp,Action,EntityType,EntityId,EntityName,UserId,UserEmail,UserRoles,IsImpersonating,OriginalUserEmail,HttpMethod,Endpoint,IpAddress,Success,ErrorMessage,Description");

            // Data rows
            foreach (var log in auditLogs)
            {
                csv.AppendLine($"{log.Timestamp:yyyy-MM-dd HH:mm:ss},{EscapeCsvField(log.Action)},{EscapeCsvField(log.EntityType)},{log.EntityId},{EscapeCsvField(log.EntityName)},{log.UserId},{EscapeCsvField(log.UserEmail)},{EscapeCsvField(log.UserRoles)},{log.IsImpersonating},{EscapeCsvField(log.OriginalUserEmail)},{EscapeCsvField(log.HttpMethod)},{EscapeCsvField(log.Endpoint)},{EscapeCsvField(log.IpAddress)},{log.Success},{EscapeCsvField(log.ErrorMessage)},{EscapeCsvField(log.Description)}");
            }

            return csv.ToString();
        }

        private string EscapeCsvField(string? field)
        {
            if (string.IsNullOrEmpty(field))
                return string.Empty;

            if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }

            return field;
        }
    }
}