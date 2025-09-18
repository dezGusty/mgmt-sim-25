using ManagementSimulator.Database.Entities;
using ManagementSimulator.Core.Dtos.Requests.PagedQueryParams;
using ManagementSimulator.Core.Dtos.Responses;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ManagementSimulator.Core.Services.Interfaces
{
    public interface IAuditLogService
    {
        /// <summary>
        /// Logs an action with full context information
        /// </summary>
        Task LogActionAsync(
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
            string? description = null);

        /// <summary>
        /// Logs a create action
        /// </summary>
        Task LogCreateAsync<T>(T entity, ClaimsPrincipal? user = null, HttpContext? httpContext = null) where T : class;

        /// <summary>
        /// Logs an update action with before and after states
        /// </summary>
        Task LogUpdateAsync<T>(T oldEntity, T newEntity, ClaimsPrincipal? user = null, HttpContext? httpContext = null) where T : class;

        /// <summary>
        /// Logs a delete action
        /// </summary>
        Task LogDeleteAsync<T>(T entity, ClaimsPrincipal? user = null, HttpContext? httpContext = null) where T : class;

        /// <summary>
        /// Logs authentication events (login, logout, impersonation)
        /// </summary>
        Task LogAuthenticationAsync(
            string action,
            string userEmail,
            int userId,
            bool success = true,
            string? errorMessage = null,
            HttpContext? httpContext = null,
            int? impersonatedUserId = null,
            string? impersonatedUserEmail = null);

        /// <summary>
        /// Logs HTTP requests automatically
        /// </summary>
        Task LogHttpRequestAsync(
            HttpContext httpContext,
            string action,
            bool success = true,
            string? errorMessage = null,
            object? additionalData = null);

        /// <summary>
        /// Gets paginated audit logs with optional filtering
        /// </summary>
        Task<IFilteredApiResponse<AuditLog>> GetAuditLogsAsync(
            int page = 1,
            int pageSize = 50,
            string? userId = null,
            string? action = null,
            string? entityType = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? search = null);

        /// <summary>
        /// Gets audit logs for a specific entity
        /// </summary>
        Task<IEnumerable<AuditLog>> GetEntityAuditTrailAsync(string entityType, int entityId);

        /// <summary>
        /// Gets audit logs for a specific user
        /// </summary>
        Task<IEnumerable<AuditLog>> GetUserAuditTrailAsync(int userId, DateTime? startDate = null, DateTime? endDate = null);
    }
}