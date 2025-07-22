using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManagementSimulator.Database.Repositories.Intefaces;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Context;
using Microsoft.EntityFrameworkCore;


namespace ManagementSimulator.Database.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        private readonly MGMTSimulatorDbContext _dbContext;
        public UserRepository(MGMTSimulatorDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            return await _dbContext.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<List<User>> GetAllUsersWithReferencesAsync()
        {
            return await _dbContext.Users
                .Include(u => u.Roles)
                .Include(u => u.Title)
                .ToListAsync();
        }

        public async Task<User?> GetUserWithReferencesByIdAsync(int id)
        {
            return await _dbContext.Users
                .Include(u => u.Roles)
                    .ThenInclude(ru => ru.Role)
                .Include(u => u.Title)
                .FirstOrDefaultAsync(u => u.Id == id);
        }
        public async Task<List<User>> GetUsersByManagerIdAsync(int managerId)
        {
            return await _dbContext.EmployeeManagers
                                .Where(em => em.ManagerId == managerId)
                                 .Select(em => em.Employee)
                                 .ToListAsync();
        }
}
}
