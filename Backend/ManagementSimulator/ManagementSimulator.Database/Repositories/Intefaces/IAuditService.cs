using ManagementSimulator.Database.Entities;

namespace ManagementSimulator.Database.Repositories.Intefaces
{
    public interface IAuditService
    {
        /// <summary>
        /// Gets the current user ID from the HTTP context
        /// </summary>
        /// <returns>Current user ID or null if not authenticated</returns>
        int? GetCurrentUserId();

        /// <summary>
        /// Sets audit properties for entity creation
        /// </summary>
        /// <param name="entity">Entity to audit</param>
        /// <param name="userId">User ID performing the action (optional, will use current user if not provided)</param>
        void SetCreatedAudit(BaseEntity entity, int? userId = null);

        /// <summary>
        /// Sets audit properties for entity modification
        /// </summary>
        /// <param name="entity">Entity to audit</param>
        /// <param name="userId">User ID performing the action (optional, will use current user if not provided)</param>
        void SetModifiedAudit(BaseEntity entity, int? userId = null);

        /// <summary>
        /// Sets audit properties for entity deletion
        /// </summary>
        /// <param name="entity">Entity to audit</param>
        /// <param name="userId">User ID performing the action (optional, will use current user if not provided)</param>
        void SetDeletedAudit(BaseEntity entity, int? userId = null);

        /// <summary>
        /// Gets user information for audit display purposes
        /// </summary>
        /// <param name="userId">User ID to get information for</param>
        /// <returns>User email or identifier for display</returns>
        Task<string?> GetUserDisplayNameAsync(int userId);

        /// <summary>
        /// Gets audit information for an entity
        /// </summary>
        /// <param name="entity">Entity to get audit info for</param>
        /// <returns>Audit information including user names</returns>
        Task<AuditInfo> GetAuditInfoAsync(BaseEntity entity);
    }

    public class AuditInfo
    {
        public string? CreatedByDisplayName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ModifiedByDisplayName { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? DeletedByDisplayName { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}