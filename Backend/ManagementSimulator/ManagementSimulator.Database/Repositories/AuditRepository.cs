using ManagementSimulator.Database.Context;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Repositories.Intefaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManagementSimulator.Database.Repositories
{
    public class AuditRepository : IAuditRepository
    {
        private readonly MGMTSimulatorDbContext _context;

        public AuditRepository(MGMTSimulatorDbContext context)
        {
            _context = context;
        }

        public async Task<List<AuditLog>> GetAuditLogsByEntityAsync(string entityType, int? entityId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.AuditLogs.AsQueryable();

            query = query.Where(log => log.EntityType == entityType);

            if (entityId.HasValue)
            {
                query = query.Where(log => log.EntityId == entityId.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(log => log.Timestamp >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(log => log.Timestamp <= endDate.Value);
            }

            return await query
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetAuditLogsByUserAsync(int userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.AuditLogs.AsQueryable();

            query = query.Where(log => log.UserId == userId);

            if (startDate.HasValue)
            {
                query = query.Where(log => log.Timestamp >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(log => log.Timestamp <= endDate.Value);
            }

            return await query
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetAuditLogsAsync(DateTime? startDate = null, DateTime? endDate = null, int skip = 0, int take = 100)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(log => log.Timestamp >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(log => log.Timestamp <= endDate.Value);
            }

            return await query
                .OrderByDescending(log => log.Timestamp)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<AuditLog?> GetAuditLogByIdAsync(int id)
        {
            return await _context.AuditLogs
                .FirstOrDefaultAsync(log => log.Id == id);
        }

        public async Task<AuditLog> AddAuditLogAsync(AuditLog auditLog)
        {
            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
            return auditLog;
        }
    }
}