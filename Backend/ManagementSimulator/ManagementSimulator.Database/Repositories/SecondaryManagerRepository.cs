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
    public class SecondaryManagerRepository : ISecondaryManagerRepository
    {
        private readonly MGMTSimulatorDbContext _context;

        public SecondaryManagerRepository(MGMTSimulatorDbContext context)
        {
            _context = context;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<List<SecondaryManager>> GetAllSecondaryManagersAsync(bool includeDeleted = false, bool tracking = false)
        {
            IQueryable<SecondaryManager> query = _context.SecondaryManagers;

            if (!tracking)
                query = query.AsNoTracking();

            if (!includeDeleted)
                query = query.Where(sm => sm.DeletedAt == null);

            return await query
                .Include(sm => sm.Employee)
                .Include(sm => sm.Manager)
                .Include(sm => sm.AssignedByAdmin)
                .ToListAsync();
        }

        public async Task<List<SecondaryManager>> GetActiveSecondaryManagersForEmployeeAsync(int employeeId, bool tracking = false)
        {
            IQueryable<SecondaryManager> query = _context.SecondaryManagers;

            if (!tracking)
                query = query.AsNoTracking();

            var now = DateTime.UtcNow;

            return await query
                .Where(sm => sm.EmployeeId == employeeId)
                .Where(sm => sm.DeletedAt == null)
                .Where(sm => sm.StartDate <= now && sm.EndDate >= now)
                .Include(sm => sm.Manager)
                .Include(sm => sm.AssignedByAdmin)
                .ToListAsync();
        }

        public async Task<List<SecondaryManager>> GetSecondaryManagersForEmployeeAsync(int employeeId, bool includeDeleted = false, bool tracking = false)
        {
            IQueryable<SecondaryManager> query = _context.SecondaryManagers;

            if (!tracking)
                query = query.AsNoTracking();

            if (!includeDeleted)
                query = query.Where(sm => sm.DeletedAt == null);

            return await query
                .Where(sm => sm.EmployeeId == employeeId)
                .Include(sm => sm.Manager)
                .Include(sm => sm.AssignedByAdmin)
                .OrderByDescending(sm => sm.StartDate)
                .ToListAsync();
        }

        public async Task<List<User>> GetEmployeesWithActiveSecondaryManagerAsync(int secondaryManagerId, bool tracking = false)
        {
            IQueryable<SecondaryManager> query = _context.SecondaryManagers;

            if (!tracking)
                query = query.AsNoTracking();

            var now = DateTime.UtcNow;

            return await query
                .Where(sm => sm.SecondaryManagerId == secondaryManagerId)
                .Where(sm => sm.DeletedAt == null)
                .Where(sm => sm.StartDate <= now && sm.EndDate >= now)
                .Include(sm => sm.Employee)
                .Include(sm => sm.Employee.Title)
                .Include(sm => sm.Employee.Roles)
                    .ThenInclude(eru => eru.Role)
                .Select(sm => sm.Employee)
                .ToListAsync();
        }

        public async Task<SecondaryManager?> GetSecondaryManagerByIdAsync(int employeeId, int secondaryManagerId, DateTime startDate, bool includeDeleted = false, bool tracking = false)
        {
            IQueryable<SecondaryManager> query = _context.SecondaryManagers;

            if (!tracking)
                query = query.AsNoTracking();

            if (!includeDeleted)
                query = query.Where(sm => sm.DeletedAt == null);

            return await query
                .Where(sm => sm.EmployeeId == employeeId && sm.SecondaryManagerId == secondaryManagerId && sm.StartDate == startDate)
                .Include(sm => sm.Employee)
                .Include(sm => sm.Manager)
                .Include(sm => sm.AssignedByAdmin)
                .FirstOrDefaultAsync();
        }

        public async Task<List<SecondaryManager>> GetSecondaryManagersAssignedByAdminAsync(int adminId, bool includeDeleted = false, bool tracking = false)
        {
            IQueryable<SecondaryManager> query = _context.SecondaryManagers;

            if (!tracking)
                query = query.AsNoTracking();

            if (!includeDeleted)
                query = query.Where(sm => sm.DeletedAt == null);

            return await query
                .Where(sm => sm.AssignedByAdminId == adminId)
                .Include(sm => sm.Employee)
                .Include(sm => sm.Manager)
                .OrderByDescending(sm => sm.CreatedAt)
                .ToListAsync();
        }

        public async Task AddSecondaryManagerAsync(SecondaryManager secondaryManager)
        {
            _context.SecondaryManagers.Add(secondaryManager);
            await SaveChangesAsync();
        }

        public async Task UpdateSecondaryManagerAsync(SecondaryManager secondaryManager)
        {
            secondaryManager.ModifiedAt = DateTime.UtcNow;
            _context.SecondaryManagers.Update(secondaryManager);
            await SaveChangesAsync();
        }

        public async Task DeleteSecondaryManagerAsync(int employeeId, int secondaryManagerId, DateTime startDate)
        {
            var secondaryManager = await _context.SecondaryManagers
                .Where(sm => sm.EmployeeId == employeeId && sm.SecondaryManagerId == secondaryManagerId && sm.StartDate == startDate)
                .FirstOrDefaultAsync();

            if (secondaryManager != null)
            {
                secondaryManager.DeletedAt = DateTime.UtcNow;
                secondaryManager.ModifiedAt = DateTime.UtcNow;
                await SaveChangesAsync();
            }
        }

        public async Task<List<SecondaryManager>> GetExpiredSecondaryManagersAsync(bool tracking = false)
        {
            IQueryable<SecondaryManager> query = _context.SecondaryManagers;

            if (!tracking)
                query = query.AsNoTracking();

            var now = DateTime.UtcNow;

            return await query
                .Where(sm => sm.DeletedAt == null)
                .Where(sm => sm.EndDate < now)
                .Include(sm => sm.Employee)
                .Include(sm => sm.Manager)
                .ToListAsync();
        }

        public async Task<bool> HasActiveSecondaryManagerAsync(int employeeId, int secondaryManagerId)
        {
            var now = DateTime.UtcNow;

            return await _context.SecondaryManagers
                .AnyAsync(sm => sm.EmployeeId == employeeId 
                    && sm.SecondaryManagerId == secondaryManagerId
                    && sm.DeletedAt == null
                    && sm.StartDate <= now 
                    && sm.EndDate >= now);
        }

        public async Task<bool> HasOverlappingSecondaryManagerAsync(int employeeId, int secondaryManagerId, DateTime startDate, DateTime endDate, int? excludeAssignmentId = null)
        {
            var query = _context.SecondaryManagers
                .Where(sm => sm.EmployeeId == employeeId
                    && sm.SecondaryManagerId == secondaryManagerId
                    && sm.DeletedAt == null
                    && sm.StartDate <= endDate && sm.EndDate >= startDate);

            return await query.AnyAsync();
        }

        public async Task<bool> HasActiveSecondaryManagerForEmployeeAsync(int employeeId)
        {
            var now = DateTime.UtcNow;

            return await _context.SecondaryManagers
                .AnyAsync(sm => sm.EmployeeId == employeeId
                    && sm.DeletedAt == null
                    && sm.StartDate <= now
                    && sm.EndDate >= now);
        }

        public async Task<bool> HasActiveSecondaryManagerAssignmentAsync(int userId)
        {
            var now = DateTime.UtcNow;

            return await _context.SecondaryManagers
                .AnyAsync(sm => sm.SecondaryManagerId == userId
                    && sm.DeletedAt == null
                    && sm.StartDate <= now
                    && sm.EndDate >= now);
        }
    }
}