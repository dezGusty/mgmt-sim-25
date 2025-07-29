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

        public async Task<List<User>> GetAllAdminsAsync(string? lastName, string? email)
        {
            var query = GetRecords()
                .Where(u => u.Roles.Any(r => r.Role.Rolename == "Admin"));

            // filtering 
            if (!string.IsNullOrEmpty(lastName))
            {
                query = query.Where(u => u.LastName.Contains(lastName));
            }

            if (!string.IsNullOrEmpty(email))
            {
                query = query.Where(u => u.Email.Contains(email));
            }

            return await query.ToListAsync();
        }

        public async Task<(List<User>? Data, int TotalCount)> GetAllManagersFilteredAsync(string? lastName, string? email, QueryParams parameters, bool includeDeleted = false)
        {
            IQueryable<User> query = GetRecords()
                                     .Include(u => u.Subordinates)
                                        .ThenInclude(subordinates => subordinates.Employee)
                                            .ThenInclude(e => e.Title)
                                     .Where(u => u.Subordinates.Count > 0);
            if (includeDeleted == false)
                query = query.Where(u => u.DeletedAt == null);
            
            // filtering 
            if (!string.IsNullOrEmpty(lastName))
            {
                query = query.Where(u => u.LastName.Contains(lastName));
            }
            if (!string.IsNullOrEmpty(email))
            {
                query = query.Where(u => u.Email.Contains(email));
            }

            var totalCount = await query.CountAsync();

            if (parameters == null)
                return (await query.ToListAsync(), totalCount);

            // sorting
            if (!string.IsNullOrEmpty(parameters.SortBy))
                query = query.ApplySorting<User>(parameters.SortBy, parameters.SortDescending ?? false);
            else
                query = query.OrderBy(u => u.Id);

            // paging 
            if (parameters.Page == null || parameters.Page <= 0 || parameters.PageSize == null || parameters.PageSize <= 0)
            {
                return (await query.ToListAsync(), totalCount);
            }
            else
            {
                var pagedData = await query.Skip(((int)parameters.Page - 1) * (int)parameters.PageSize)
                               .Take((int)parameters.PageSize)
                                .ToListAsync();
                return (pagedData, totalCount);
            }
        }

        public async Task<(List<User>? Data, int TotalCount)> GetAllUsersWithReferencesFilteredAsync(string? lastName, string? email, QueryParams parameters)
        {
            IQueryable<User> query = _dbContext.Users
                .Include(u => u.Roles)
                    .ThenInclude(u => u.Role)
                .Include(u => u.Title)
                    .ThenInclude(jt => jt.Department);
            // filtering 
            if (!string.IsNullOrEmpty(lastName))
            {
                query = query.Where(u => u.LastName.Contains(lastName));
            }
            if (!string.IsNullOrEmpty(email))
            {
                query = query.Where(u => u.Email.Contains(email));
            }

            var totalCount = await query.CountAsync();

            if (parameters == null)
                return (await query.ToListAsync(), totalCount);

            // sorting
            if (!string.IsNullOrEmpty(parameters.SortBy))
                query = query.ApplySorting<User>(parameters.SortBy, parameters.SortDescending ?? false);
            else
                query = query.OrderBy(u => u.Id);

            // paging 
            if (parameters.Page == null || parameters.Page <= 0 || parameters.PageSize == null || parameters.PageSize <= 0)
            {
                return (await query.ToListAsync(), totalCount);
            }
            else
            {
                var pagedData = await query.Skip(((int)parameters.Page - 1) * (int)parameters.PageSize)
                               .Take((int)parameters.PageSize)
                                .ToListAsync();
                return (pagedData, totalCount);
            }
        }

        public async Task<(List<User>? Data, int TotalCount)> GetAllUnassignedUsersFilteredAsync(QueryParams parameters)
        {
            IQueryable<User> query = _dbContext.Users
                .Include(u => u.Managers)
                     .Where(u => u.Managers.Count == 0)
                .Include(u => u.Roles)
                    .ThenInclude(u => u.Role)
                .Include(u => u.Title);

            var totalCount = await query.CountAsync();

            if (parameters == null)
                return (await query.ToListAsync(), totalCount);

            // paging 
            if (parameters.Page == null || parameters.Page <= 0 || parameters.PageSize == null || parameters.PageSize <= 0)
            {
                return (await query.ToListAsync(), totalCount);
            }
            else
            {
                var pagedData = await query.Skip(((int)parameters.Page - 1) * (int)parameters.PageSize)
                               .Take((int)parameters.PageSize)
                                .ToListAsync();
                return (pagedData, totalCount);
            }
        }
    }
}
