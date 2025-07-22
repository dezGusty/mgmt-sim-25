using ManagementSimulator.Database.Context;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Repositories.Intefaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Database.Repositories
{
    public class EmployeeRoleRepository : BaseRepository<Entities.EmployeeRole>, IEmployeeRoleRepository
    {
        private readonly MGMTSimulatorDbContext _dbContext;

        public EmployeeRoleRepository(MGMTSimulatorDbContext databaseContext) : base(databaseContext)
        {
            _dbContext = databaseContext;
        }

        public async Task AddEmployeeRoleUserAsync(EmployeeRoleUser employeeRoleUser)
        {
            await _dbContext.EmployeeRolesUsers.AddAsync(employeeRoleUser);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteEmployeeUserRoleAsync(EmployeeRoleUser employeeRoleUser)
        {
            employeeRoleUser.DeletedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }

        public async Task<EmployeeRoleUser?> GetEmployeeRoleUserIncludeDeletedAsync(int userId, int roleId)
        {
            return await _dbContext.EmployeeRolesUsers
                .FirstOrDefaultAsync(eru => eru.UsersId == userId && eru.RolesId == roleId);
        }

        public async Task<EmployeeRoleUser?> GetEmployeeRoleUserAsync(int userId, int roleId)
        {
            return await _dbContext.EmployeeRolesUsers
                .Where(eru => eru.DeletedAt == null)
                .FirstOrDefaultAsync(eru => eru.UsersId == userId && eru.RolesId == roleId);
        }

        public async Task<List<EmployeeRoleUser>> GetEmployeeRoleUsersByUserIdAsync(int userId)
        {
            return await _dbContext.EmployeeRolesUsers
                .Where(eru => eru.DeletedAt == null)
                .Where(eru => eru.UsersId == userId)
                .Include(eru => eru.Role)
                .ToListAsync(); 
        }
    }
}
