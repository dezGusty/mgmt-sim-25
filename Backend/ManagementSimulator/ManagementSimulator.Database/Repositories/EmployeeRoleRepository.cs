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
            _dbContext.EmployeeRolesUsers.Remove(employeeRoleUser);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<EmployeeRole>> GetAllUserRolesAsync(bool includeDeleted = false, bool tracking = false)
        {
            IQueryable<EmployeeRole> query = _dbContext.EmployeeRoles;

            if(!tracking)
                query = query.AsNoTracking();

            if (!includeDeleted)
                query = query.Where(eru => eru.DeletedAt == null);

            return await query.ToListAsync();
        }

        public async Task<EmployeeRoleUser?> GetEmployeeRoleUserAsync(int userId, int roleId, bool includeDeleted = false, bool tracking = false)
        {
            IQueryable<EmployeeRoleUser> query = _dbContext.EmployeeRolesUsers;

            if (!tracking)
                query = query.AsNoTracking();

            if (!includeDeleted)
                query = query.Where(eru => eru.DeletedAt == null);

            return await query.FirstOrDefaultAsync(eru => eru.UsersId == userId && eru.RolesId == roleId);
        }

        public Task<int> GetEmployeeRoleUserByNameAsync(string name, bool includeDeleted = false, bool tracking = false)
        {
            IQueryable<EmployeeRole> query = _dbContext.EmployeeRoles;

            if (!tracking)
                query = query.AsNoTracking();

            if (!includeDeleted)
                query = query.Where(eru => eru.DeletedAt == null);

            return query.Where(eru => eru.Rolename == name).Select(eru => eru.Id).FirstOrDefaultAsync();
        }

        public async Task<List<EmployeeRoleUser>> GetEmployeeRoleUsersByUserIdAsync(int userId, bool includeDeleted = false, bool tracking = false)
        {
            IQueryable<EmployeeRoleUser> query = _dbContext.EmployeeRolesUsers;

            if (!tracking)
                query = query.AsNoTracking();

            if (!includeDeleted)
                query = query.Where(eru => eru.DeletedAt == null);

            query = query.Where(eru => eru.UsersId == userId)
                .Include(eru => eru.Role);
            
            return await query.ToListAsync();
        }

        public async Task<List<EmployeeRoleUser>> GetEmployeeRoleUsersByUserIdsAsync(List<int> userIds, bool includeDeleted = false, bool tracking = false)
        {
            IQueryable<EmployeeRoleUser> query = _dbContext.EmployeeRolesUsers;

            if (!tracking)
                query = query.AsNoTracking();

            if (!includeDeleted)
                query = query.Where(eru => eru.DeletedAt == null);

            query = query.Where(eru => userIds.Contains(eru.UsersId))
                .Include(eru => eru.Role);

            return await query.ToListAsync();
        }
    }
}
