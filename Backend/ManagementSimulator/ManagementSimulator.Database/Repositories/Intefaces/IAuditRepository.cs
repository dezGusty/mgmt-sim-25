using ManagementSimulator.Database.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ManagementSimulator.Database.Repositories.Intefaces
{
    public interface IAuditRepository
    {
        Task<List<AuditLog>> GetAuditLogsByEntityAsync(string entityType, int? entityId, DateTime? startDate = null, DateTime? endDate = null);
        Task<List<AuditLog>> GetAuditLogsByUserAsync(int userId, DateTime? startDate = null, DateTime? endDate = null);
        Task<List<AuditLog>> GetAuditLogsAsync(DateTime? startDate = null, DateTime? endDate = null, int skip = 0, int take = 100);
        Task<AuditLog?> GetAuditLogByIdAsync(int id);
        Task<AuditLog> AddAuditLogAsync(AuditLog auditLog);
    }
}