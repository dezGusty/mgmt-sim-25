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
            result.DeletedAt = DateTime.UtcNow;

            await SaveChangesAsync();
        }

        public async Task<List<User>> GetManagersForEmployeesByIdAsync(int subordinateId)
        {
            return await _context.EmployeeManagers
                                 .Where(em => em.DeletedAt == null)
                                 .Where(um => um.EmployeeId == subordinateId)
                                 .Include(um => um.Manager)
                                 .Include(um => um.Manager.Title)
                                 .Include(um => um.Manager.Roles)
                                     .ThenInclude(eru => eru.Role)
                                 .Select(um => um.Manager)
                                 .ToListAsync();
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
                                 .Select(um => um.Employee)
                                 .ToListAsync();
        }

        public async Task<EmployeeManager?> GetEmployeeManagersByIdAsync(int employeeId, int managerId)
        {
            return await _context.EmployeeManagers
                                 .Where(em => em.DeletedAt == null)
                                 .Where(em => em.EmployeeId == employeeId && em.ManagerId == managerId)
                                 .FirstOrDefaultAsync();
        }

        public async Task<EmployeeManager?> GetEmployeeManagersByIdIncludeDeletedAsync(int employeeId, int managerId)
        {
            return await _context.EmployeeManagers
                     .Where(em => em.EmployeeId == employeeId && em.ManagerId == managerId)
                     .FirstOrDefaultAsync();
        }
    }
}
