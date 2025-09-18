using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Repositories.Intefaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ManagementSimulator.Core.Services
{
    public class AuditService : IAuditService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int? GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }

        public void SetCreatedAudit(BaseEntity entity, int? userId = null)
        {
            var currentUserId = userId ?? GetCurrentUserId();
            entity.CreatedAt = DateTime.UtcNow;
            entity.CreatedBy = currentUserId;
        }

        public void SetModifiedAudit(BaseEntity entity, int? userId = null)
        {
            var currentUserId = userId ?? GetCurrentUserId();
            entity.ModifiedAt = DateTime.UtcNow;
            entity.ModifiedBy = currentUserId;
        }

        public void SetDeletedAudit(BaseEntity entity, int? userId = null)
        {
            var currentUserId = userId ?? GetCurrentUserId();
            entity.DeletedAt = DateTime.UtcNow;
            entity.DeletedBy = currentUserId;
        }

        public async Task<string?> GetUserDisplayNameAsync(int userId)
        {
            // Simple implementation without repository dependency
            return await Task.FromResult($"User #{userId}");
        }

        public async Task<AuditInfo> GetAuditInfoAsync(BaseEntity entity)
        {
            var auditInfo = new AuditInfo
            {
                CreatedAt = entity.CreatedAt,
                ModifiedAt = entity.ModifiedAt,
                DeletedAt = entity.DeletedAt
            };

            // Get user display names for audit properties
            if (entity.CreatedBy.HasValue)
            {
                auditInfo.CreatedByDisplayName = await GetUserDisplayNameAsync(entity.CreatedBy.Value);
            }

            if (entity.ModifiedBy.HasValue)
            {
                auditInfo.ModifiedByDisplayName = await GetUserDisplayNameAsync(entity.ModifiedBy.Value);
            }

            if (entity.DeletedBy.HasValue)
            {
                auditInfo.DeletedByDisplayName = await GetUserDisplayNameAsync(entity.DeletedBy.Value);
            }

            return auditInfo;
        }
    }
}