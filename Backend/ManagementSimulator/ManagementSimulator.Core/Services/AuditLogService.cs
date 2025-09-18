using ManagementSimulator.Core.Dtos.Requests.PagedQueryParams;
using ManagementSimulator.Core.Dtos.Responses;
using ManagementSimulator.Core.Services.Interfaces;
using ManagementSimulator.Database.Context;
using ManagementSimulator.Database.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text.Json;
using System.Reflection;

namespace ManagementSimulator.Core.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly MGMTSimulatorDbContext _context;
        private readonly ILogger<AuditLogService> _logger;

        public AuditLogService(MGMTSimulatorDbContext context, ILogger<AuditLogService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task LogActionAsync(
            string action,
            string entityType,
            int? entityId = null,
            string? entityName = null,
            object? oldValues = null,
            object? newValues = null,
            object? additionalData = null,
            ClaimsPrincipal? user = null,
            HttpContext? httpContext = null,
            bool success = true,
            string? errorMessage = null,
            string? description = null)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId,
                    EntityName = entityName ?? string.Empty,
                    Success = success,
                    ErrorMessage = errorMessage,
                    Description = description,
                    Timestamp = DateTime.UtcNow
                };

                // Extract user information
                if (user != null)
                {
                    ExtractUserContext(auditLog, user);
                }

                // Extract HTTP context information
                if (httpContext != null)
                {
                    ExtractHttpContext(auditLog, httpContext);
                }

                // Serialize data to JSON
                if (oldValues != null)
                {
                    auditLog.OldValues = JsonSerializer.Serialize(oldValues, new JsonSerializerOptions { WriteIndented = true });
                }

                if (newValues != null)
                {
                    auditLog.NewValues = JsonSerializer.Serialize(newValues, new JsonSerializerOptions { WriteIndented = true });
                }

                if (additionalData != null)
                {
                    auditLog.AdditionalData = JsonSerializer.Serialize(additionalData, new JsonSerializerOptions { WriteIndented = true });
                }

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Don't let audit logging failures break the application
                _logger.LogError(ex, "Failed to write audit log for action {Action} on {EntityType}", action, entityType);
            }
        }

        public async Task LogCreateAsync<T>(T entity, ClaimsPrincipal? user = null, HttpContext? httpContext = null) where T : class
        {
            var entityType = typeof(T).Name;
            var entityId = GetEntityId(entity);
            var entityName = GetEntityName(entity);

            await LogActionAsync(
                "CREATE",
                entityType,
                entityId,
                entityName,
                oldValues: null,
                newValues: entity,
                user: user,
                httpContext: httpContext,
                description: $"Created {entityType} '{entityName}'"
            );
        }

        public async Task LogUpdateAsync<T>(T oldEntity, T newEntity, ClaimsPrincipal? user = null, HttpContext? httpContext = null) where T : class
        {
            var entityType = typeof(T).Name;
            var entityId = GetEntityId(newEntity);
            var entityName = GetEntityName(newEntity);

            await LogActionAsync(
                "UPDATE",
                entityType,
                entityId,
                entityName,
                oldValues: oldEntity,
                newValues: newEntity,
                user: user,
                httpContext: httpContext,
                description: $"Updated {entityType} '{entityName}'"
            );
        }

        public async Task LogDeleteAsync<T>(T entity, ClaimsPrincipal? user = null, HttpContext? httpContext = null) where T : class
        {
            var entityType = typeof(T).Name;
            var entityId = GetEntityId(entity);
            var entityName = GetEntityName(entity);

            await LogActionAsync(
                "DELETE",
                entityType,
                entityId,
                entityName,
                oldValues: entity,
                newValues: null,
                user: user,
                httpContext: httpContext,
                description: $"Deleted {entityType} '{entityName}'"
            );
        }

        public async Task LogAuthenticationAsync(
            string action,
            string userEmail,
            int userId,
            bool success = true,
            string? errorMessage = null,
            HttpContext? httpContext = null,
            int? impersonatedUserId = null,
            string? impersonatedUserEmail = null)
        {
            var auditLog = new AuditLog
            {
                Action = action,
                EntityType = "Authentication",
                EntityId = userId,
                EntityName = userEmail,
                UserId = userId,
                UserEmail = userEmail,
                Success = success,
                ErrorMessage = errorMessage,
                Timestamp = DateTime.UtcNow,
                Description = $"Authentication event: {action} for user {userEmail}"
            };

            // Handle impersonation context
            if (impersonatedUserId.HasValue && !string.IsNullOrEmpty(impersonatedUserEmail))
            {
                auditLog.IsImpersonating = true;
                auditLog.OriginalUserId = userId;
                auditLog.OriginalUserEmail = userEmail;
                auditLog.UserId = impersonatedUserId.Value;
                auditLog.UserEmail = impersonatedUserEmail;
                auditLog.Description = $"Impersonation event: {action} - Admin {userEmail} acting as {impersonatedUserEmail}";
            }

            if (httpContext != null)
            {
                ExtractHttpContext(auditLog, httpContext);
            }

            try
            {
                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write authentication audit log for {Action} by {UserEmail}", action, userEmail);
            }
        }

        public async Task LogHttpRequestAsync(
            HttpContext httpContext,
            string action,
            bool success = true,
            string? errorMessage = null,
            object? additionalData = null)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    Action = action,
                    EntityType = "HttpRequest",
                    Success = success,
                    ErrorMessage = errorMessage,
                    Timestamp = DateTime.UtcNow
                };

                ExtractUserContext(auditLog, httpContext.User);
                ExtractHttpContext(auditLog, httpContext);

                if (additionalData != null)
                {
                    auditLog.AdditionalData = JsonSerializer.Serialize(additionalData, new JsonSerializerOptions { WriteIndented = true });
                }

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write HTTP request audit log for {Action}", action);
            }
        }

        public async Task<IFilteredApiResponse<AuditLog>> GetAuditLogsAsync(
            int page = 1,
            int pageSize = 50,
            string? userId = null,
            string? action = null,
            string? entityType = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? search = null)
        {
            var query = _context.AuditLogs
                .Include(a => a.User)
                .Include(a => a.OriginalUser)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out var userIdInt))
            {
                query = query.Where(a => a.UserId == userIdInt || a.OriginalUserId == userIdInt);
            }

            if (!string.IsNullOrEmpty(action))
            {
                query = query.Where(a => a.Action.Contains(action));
            }

            if (!string.IsNullOrEmpty(entityType))
            {
                query = query.Where(a => a.EntityType.Contains(entityType));
            }

            if (startDate.HasValue)
            {
                query = query.Where(a => a.Timestamp >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(a => a.Timestamp <= endDate.Value);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(a =>
                    a.UserEmail.Contains(search) ||
                    a.EntityName.Contains(search) ||
                    a.Description!.Contains(search) ||
                    a.Endpoint.Contains(search));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var items = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new FilteredApiResponse<AuditLog>
            {
                Data = items,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        public async Task<IEnumerable<AuditLog>> GetEntityAuditTrailAsync(string entityType, int entityId)
        {
            return await _context.AuditLogs
                .Include(a => a.User)
                .Include(a => a.OriginalUser)
                .Where(a => a.EntityType == entityType && a.EntityId == entityId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetUserAuditTrailAsync(int userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.AuditLogs
                .Include(a => a.User)
                .Include(a => a.OriginalUser)
                .Where(a => a.UserId == userId || a.OriginalUserId == userId);

            if (startDate.HasValue)
            {
                query = query.Where(a => a.Timestamp >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(a => a.Timestamp <= endDate.Value);
            }

            return await query
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        private void ExtractUserContext(AuditLog auditLog, ClaimsPrincipal user)
        {
            if (user.Identity == null || !user.Identity.IsAuthenticated)
            {
                auditLog.UserId = 0;
                auditLog.UserEmail = "Anonymous";
                auditLog.UserRoles = "None";
                return;
            }

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out var userId))
            {
                auditLog.UserId = userId;
            }

            auditLog.UserEmail = user.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown";
            auditLog.UserRoles = string.Join(",", user.FindAll(ClaimTypes.Role).Select(r => r.Value));

            // Handle impersonation
            var isImpersonating = user.FindFirst("IsImpersonating")?.Value == "true";
            if (isImpersonating)
            {
                auditLog.IsImpersonating = true;
                var originalUserIdClaim = user.FindFirst("OriginalUserId")?.Value;
                if (int.TryParse(originalUserIdClaim, out var originalUserId))
                {
                    auditLog.OriginalUserId = originalUserId;
                }
                auditLog.OriginalUserEmail = user.FindFirst("OriginalUserEmail")?.Value;

                // When impersonating, the effective user is the impersonated user
                var impersonatedUserIdClaim = user.FindFirst("ImpersonatedUserId")?.Value;
                if (int.TryParse(impersonatedUserIdClaim, out var impersonatedUserId))
                {
                    auditLog.UserId = impersonatedUserId;
                }
            }
        }

        private void ExtractHttpContext(AuditLog auditLog, HttpContext httpContext)
        {
            auditLog.HttpMethod = httpContext.Request.Method;
            auditLog.Endpoint = httpContext.Request.Path.Value ?? string.Empty;

            if (httpContext.Request.QueryString.HasValue)
            {
                auditLog.Endpoint += httpContext.Request.QueryString.Value;
            }

            auditLog.IpAddress = httpContext.Connection.RemoteIpAddress?.ToString();
            auditLog.UserAgent = httpContext.Request.Headers["User-Agent"].FirstOrDefault();
        }

        private int? GetEntityId(object entity)
        {
            var idProperty = entity.GetType().GetProperty("Id");
            if (idProperty != null && idProperty.PropertyType == typeof(int))
            {
                return (int?)idProperty.GetValue(entity);
            }
            return null;
        }

        private string GetEntityName(object entity)
        {
            // Try common name properties
            var nameProperties = new[] { "Name", "Title", "Email", "Rolename" };

            foreach (var propName in nameProperties)
            {
                var property = entity.GetType().GetProperty(propName);
                if (property != null && property.PropertyType == typeof(string))
                {
                    var value = property.GetValue(entity) as string;
                    if (!string.IsNullOrEmpty(value))
                    {
                        return value;
                    }
                }
            }

            return entity.GetType().Name;
        }
    }

    // Helper class for the filtered response
    public class FilteredApiResponse<T> : IFilteredApiResponse<T>
    {
        public IEnumerable<T> Data { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}