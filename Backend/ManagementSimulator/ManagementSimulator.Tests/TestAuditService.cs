using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Repositories.Intefaces;

namespace ManagementSimulator.Tests
{
    public class TestAuditService : IAuditService
    {
        private int? _currentUserId;

        public TestAuditService(int? currentUserId = null)
        {
            _currentUserId = currentUserId;
        }

        public int? GetCurrentUserId()
        {
            return _currentUserId;
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
            return await Task.FromResult($"TestUser #{userId}");
        }

        public async Task<AuditInfo> GetAuditInfoAsync(BaseEntity entity)
        {
            var auditInfo = new AuditInfo
            {
                CreatedAt = entity.CreatedAt,
                ModifiedAt = entity.ModifiedAt,
                DeletedAt = entity.DeletedAt
            };

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