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

            return await query
                .Include(sm => sm.Employee)
                .Include(sm => sm.Manager)
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
                .Where(sm => sm.StartDate <= now && sm.EndDate >= now)
                .Include(sm => sm.Manager)
                .ToListAsync();
        }

        public async Task<List<SecondaryManager>> GetSecondaryManagersForEmployeeAsync(int employeeId, bool includeDeleted = false, bool tracking = false)
        {
            IQueryable<SecondaryManager> query = _context.SecondaryManagers;

            if (!tracking)
                query = query.AsNoTracking();

            return await query
                .Where(sm => sm.EmployeeId == employeeId)
                .Include(sm => sm.Manager)
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
                .Where(sm => sm.ManagerId == secondaryManagerId)
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

            return await query
                .Where(sm => sm.EmployeeId == employeeId && sm.ManagerId == secondaryManagerId && sm.StartDate == startDate)
                .Include(sm => sm.Employee)
                .Include(sm => sm.Manager)
                .FirstOrDefaultAsync();
        }



        public async Task AddSecondaryManagerAsync(SecondaryManager secondaryManager)
        {
            _context.SecondaryManagers.Add(secondaryManager);
            await SaveChangesAsync();
        }

        public async Task UpdateSecondaryManagerAsync(SecondaryManager secondaryManager)
        {
            _context.SecondaryManagers.Update(secondaryManager);
            await SaveChangesAsync();
        }

        public async Task DeleteSecondaryManagerAsync(int employeeId, int secondaryManagerId, DateTime startDate)
        {
            var secondaryManager = await _context.SecondaryManagers
                .Where(sm => sm.EmployeeId == employeeId && sm.ManagerId == secondaryManagerId && sm.StartDate == startDate)
                .FirstOrDefaultAsync();

            if (secondaryManager != null)
            {
                _context.SecondaryManagers.Remove(secondaryManager);
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
                    && sm.ManagerId == secondaryManagerId
                    && sm.StartDate <= now 
                    && sm.EndDate >= now);
        }

        public async Task<bool> HasOverlappingSecondaryManagerAsync(int employeeId, int secondaryManagerId, DateTime startDate, DateTime endDate, int? excludeAssignmentId = null)
        {
            var query = _context.SecondaryManagers
                .Where(sm => sm.EmployeeId == employeeId
                    && sm.ManagerId == secondaryManagerId
                    && sm.StartDate <= endDate && sm.EndDate >= startDate);

            return await query.AnyAsync();
        }

        public async Task<bool> HasActiveSecondaryManagerForEmployeeAsync(int employeeId)
        {
            var now = DateTime.UtcNow;

            return await _context.SecondaryManagers
                .AnyAsync(sm => sm.EmployeeId == employeeId
                    && sm.StartDate <= now
                    && sm.EndDate >= now);
        }

        public async Task<bool> HasActiveSecondaryManagerAssignmentAsync(int userId)
        {
            var now = DateTime.UtcNow;

            return await _context.SecondaryManagers
                .AnyAsync(sm => sm.ManagerId == userId
                    && sm.StartDate <= now
                    && sm.EndDate >= now);
        }
    }
}