using ManagementSimulator.Database.Context;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Repositories.Intefaces;
using ManagementSimulator.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Database.Repositories
{
    public class EmployeeManagerRepository : IEmployeeManagerRepository
    {
        private readonly MGMTSimulatorDbContext _context;
        public EmployeeManagerRepository(MGMTSimulatorDbContext context)
        {
            _context = context;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task AddEmployeeManagersAsync(int subOrdinateId, int managerId)
        {
            EmployeeManager um = new EmployeeManager
            {
                EmployeeId = subOrdinateId,
                ManagerId = managerId,
            };
            _context.Add(um);
            await SaveChangesAsync();
        }

        public async Task DeleteEmployeeManagerAsync(int employeeId,int managerId)
        {
            var result = await _context.EmployeeManagers
                .Where(em => em.ManagerId == managerId && em.EmployeeId == employeeId)
                .FirstOrDefaultAsync();
            result!.DeletedAt = DateTime.UtcNow;

            await SaveChangesAsync();
        }

        public async Task<List<User>> GetManagersForEmployeesByIdAsync(int subordinateId)
        {
            return await _context.EmployeeManagers
                                 .Where(em => em.DeletedAt == null)
                                 .Where(em => em.EmployeeId == subordinateId)
                                 .Include(em => em.Manager)
                                 .Include(em => em.Manager.Title)
                                 .Include(em => em.Manager.Roles)
                                     .ThenInclude(eru => eru.Role)
                                 .Where(em => em.Manager != null && em.Manager.DeletedAt == null)
                                 .Select(em => em.Manager)
                                 .ToListAsync();
        }

        public async Task<List<EmployeeManager>> GetEMRelationshipForEmployeesByIdAsync(int subordinateId, bool includeDeleted = false)
        {
            IQueryable<EmployeeManager> query = _context.EmployeeManagers;
            if (!includeDeleted)
                query = query.Where(em => em.DeletedAt == null);

            query = query.Where(um => um.EmployeeId == subordinateId);

            return await query.ToListAsync();
        }

        public async Task<List<User>> GetEmployeesForManagerByIdAsync(int managerId)
        {
            return await _context.EmployeeManagers
                                 .Where(em => em.DeletedAt == null)
                                 .Where(um => um.ManagerId == managerId)
                                 .Include(um => um.Employee)
                                 .Include(um => um.Employee.Title)
                                 .Include(um => um.Employee.Roles)
                                     .ThenInclude(eru => eru.Role)
                                 .Where(um => um.Employee != null && um.Employee.DeletedAt == null)
                                 .Select(um => um.Employee)
                                 .ToListAsync();
        }

        public async Task<EmployeeManager?> GetEmployeeManagersByIdAsync(int employeeId, int managerId, bool includeDeleted = false)
        {
            IQueryable<EmployeeManager?> query = _context.EmployeeManagers;
            if (!includeDeleted)
                query = query.Where(em => em.DeletedAt == null);

            query = query.Where(em => em.EmployeeId == employeeId && em.ManagerId == managerId);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<EmployeeManager>> GetAllEmployeeManagersAsync(bool includeDeleted = false)
        {
            IQueryable<EmployeeManager> query = _context.EmployeeManagers;

            if (!includeDeleted)
                query = query.Where(em => em.DeletedAt == null);

            return await query.ToListAsync();
        }
    }
}
