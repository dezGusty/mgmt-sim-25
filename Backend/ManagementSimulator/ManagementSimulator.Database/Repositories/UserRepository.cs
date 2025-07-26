using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManagementSimulator.Database.Repositories.Intefaces;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Context;
using Microsoft.EntityFrameworkCore;
using ManagementSimulator.Database.Dtos.QueryParams;
using Microsoft.IdentityModel.Tokens;
using ManagementSimulator.Database.Extensions;


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
                .Where(u => u.DeletedAt == null)
                .Include(u => u.Roles)
                    .ThenInclude(r => r.Role)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<List<User>> GetAllUsersWithReferencesAsync()
        {
            // Deleted included
            return await _dbContext.Users
                .Include(u => u.Roles)
                    .ThenInclude(u => u.Role)
                .Include(u => u.Title)
                    .ThenInclude(jt => jt.Department)
                .ToListAsync();
        }

        public async Task<User?> GetUserWithReferencesByIdAsync(int id)
        {
            return await _dbContext.Users
                .Where(u => u.DeletedAt == null)
                .Include(u => u.Roles)
                    .ThenInclude(ru => ru.Role)
                .Include(u => u.Title)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<List<User>> GetUsersByManagerIdAsync(int managerId)
        {
            return await _dbContext.EmployeeManagers
                                .Where(u => u.DeletedAt == null)
                                .Where(em => em.ManagerId == managerId)
                                 .Select(em => em.Employee)
                                 .ToListAsync();
        }

        public async Task<bool> RestoreUserByIdAsync(int id)
        {
            return await _dbContext.Users
                .Where(u => u.Id == id && u.DeletedAt != null)
                .ExecuteUpdateAsync(u => u.SetProperty(x => x.DeletedAt, _ => null)) > 0;
        }


        public async Task<User?> GetUserByIdIncludeDeletedAsync(int id)
        {
            return await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _dbContext.Users
                .Where(u => u.DeletedAt == null)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<List<User>> GetAllUsersIncludeRelationships()
        {
            return await _dbContext.Users
                .Where(u => u.DeletedAt == null)
                .Include(u => u.Roles)
                    .ThenInclude(r => r.Role)
                .Include(u => u.Title)
                .Include(u => u.Subordinates)
                    .ThenInclude(em => em.Employee)
                        .ThenInclude(u => u.Title)
                 .ToListAsync();
        }

        public async Task<List<User>?> GetSubordinatesByUserIdsAsync(List<int> ids)
        {
            return await _dbContext.Users
                .Where(u => u.DeletedAt == null)
                .Where(u => ids.Contains(u.Id))
                .Include(u => u.Subordinates)
                    .ThenInclude(em => em.Employee)
                .ToListAsync();
        }

        public async Task<List<User>?> GetManagersByUserIdsAsync(List<int> ids)
        {
            return await _dbContext.Users
                .Where(u => u.DeletedAt == null)
                .Where(u => ids.Contains(u.Id))
                .Include(u => u.Managers)
                    .ThenInclude(em => em.Manager)
                .ToListAsync();
        }

        public async Task<List<User>?> GetAllUsersFilteredAsync(string? lastName, string? email, QueryParams parameters)
        {
            IQueryable<User> query = GetRecords(includeDeletedEntities: true);

            // filtering 
            if (!lastName.IsNullOrEmpty())
            {
                query = query.Where(u => u.LastName.Contains(lastName));
            }

            if (!email.IsNullOrEmpty())
            {
                query = query.Where(u => u.Email.Contains(email));
            }

            // sorting
            if (parameters == null)
                return await query.ToListAsync();

            if (!parameters.SortBy.IsNullOrEmpty())
                query = query.ApplySorting<User>(parameters.SortBy, parameters.SortDescending ?? false);
            else
                query = query.OrderBy(u => u.Id);

            // paging 
            if (parameters.Page == null || parameters.Page <= 0 || parameters.PageSize == null || parameters.PageSize <= 0)
            {
                return await query.ToListAsync();
            }
            else
            {
                return await query.Skip(((int)parameters.Page - 1) * (int)parameters.PageSize)
                                   .Take((int)parameters.PageSize)
                                    .ToListAsync();
            }
        }
    }
}
