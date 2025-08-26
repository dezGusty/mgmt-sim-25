using ManagementSimulator.Database.Context;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ManagementSimulator.Database.Repositories
{
    public class SecondManagerRepository : ISecondManagerRepository
    {
        private readonly MGMTSimulatorDbContext _context;

        public SecondManagerRepository(MGMTSimulatorDbContext context)
        {
            _context = context;
        }

        public async Task<List<SecondManager>> GetAllSecondManagersAsync()
        {
            return await _context.SecondManagers
                .Include(sm => sm.SecondManagerEmployee)
                .Include(sm => sm.ReplacedManager)
                .ToListAsync();
        }

        public async Task<List<SecondManager>> GetActiveSecondManagersAsync()
        {
            var now = DateTime.Now;
            return await _context.SecondManagers
                .Include(sm => sm.SecondManagerEmployee)
                .Include(sm => sm.ReplacedManager)
                .Where(sm => sm.StartDate <= now && sm.EndDate >= now)
                .ToListAsync();
        }

        public async Task<List<SecondManager>> GetSecondManagersByReplacedManagerIdAsync(int replacedManagerId)
        {
            return await _context.SecondManagers
                .Include(sm => sm.SecondManagerEmployee)
                .Include(sm => sm.ReplacedManager)
                .Where(sm => sm.ReplacedManagerId == replacedManagerId)
                .ToListAsync();
        }

        public async Task<SecondManager?> GetSecondManagerAsync(int secondManagerEmployeeId, int replacedManagerId, DateTime startDate)
        {
            return await _context.SecondManagers
                .Include(sm => sm.SecondManagerEmployee)
                .Include(sm => sm.ReplacedManager)
                .FirstOrDefaultAsync(sm => sm.SecondManagerEmployeeId == secondManagerEmployeeId
                                         && sm.ReplacedManagerId == replacedManagerId
                                         && sm.StartDate == startDate);
        }

        public async Task AddSecondManagerAsync(SecondManager secondManager)
        {
            _context.SecondManagers.Add(secondManager);
            await SaveChangesAsync();
        }

        public async Task UpdateSecondManagerAsync(SecondManager secondManager)
        {
            _context.SecondManagers.Update(secondManager);
            await SaveChangesAsync();
        }

        public async Task DeleteSecondManagerAsync(int secondManagerEmployeeId, int replacedManagerId, DateTime startDate)
        {
            var secondManager = await GetSecondManagerAsync(secondManagerEmployeeId, replacedManagerId, startDate);
            if (secondManager != null)
            {
                _context.SecondManagers.Remove(secondManager);
                await SaveChangesAsync();
            }
        }

        public async Task<List<User>> GetEmployeesForActiveSecondManagerAsync(int secondManagerEmployeeId)
        {
            var now = DateTime.Now;

            var activeReplacements = await _context.SecondManagers
                .Where(sm => sm.SecondManagerEmployeeId == secondManagerEmployeeId
                           && sm.StartDate <= now
                           && sm.EndDate >= now)
                .Select(sm => sm.ReplacedManagerId)
                .ToListAsync();

            return await _context.EmployeeManagers
                .Where(em => activeReplacements.Contains(em.ManagerId))
                .Include(em => em.Employee)
                .Select(em => em.Employee)
                .Distinct()
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}