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

        public async Task<User?> GetUserByEmail(string email, bool includeDeleted = false, bool tracking = false)
        {
            IQueryable<User> query = _dbContext.Users;

            if(!tracking) 
                query = query.AsNoTracking();

            if (!includeDeleted)
                query = query.Where(u => u.DeletedAt == null);

            query = query.Include(u => u.Roles)
                         .ThenInclude(r => r.Role);

            return await query.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<List<User>> GetAllUsersWithReferencesAsync(bool includeDeleted = false, bool tracking = false)
        {
            IQueryable<User> query = _dbContext.Users;

            if (!tracking)
                query = query.AsNoTracking();

            if (!includeDeleted)
                query = query.Where(u => u.DeletedAt == null);

            query = query.Include(u => u.Roles)
                    .ThenInclude(u => u.Role)
                .Include(u => u.Title)
                .Include(u => u.Department);

            return await query.ToListAsync();
        }

        public async Task<User?> GetUserWithReferencesByIdAsync(int id, bool includeDeleted = false, bool tracking = false)
        {
            IQueryable<User> query = _dbContext.Users;

            if (!tracking)
                query = query.AsNoTracking();

            if (!includeDeleted)
                query = query.Where(u => u.DeletedAt == null);

            query = query.Include(u => u.Roles)
                    .ThenInclude(ru => ru.Role)
                .Include(u => u.Title);

            return await query.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<List<User>> GetUsersByManagerIdAsync(int managerId, bool includeDeleted = false, bool tracking = false)
        {
            IQueryable<EmployeeManager> query = _dbContext.EmployeeManagers;

            if (!tracking)
                query = query.AsNoTracking();

            if (!includeDeleted)
                query = query.Where(u => u.DeletedAt == null);

            query = query.Where(em => em.ManagerId == managerId);
                                 

            return await query.Select(em => em.Employee).ToListAsync();
        }

        public async Task<bool> RestoreUserByIdAsync(int id)
        {
            return await _dbContext.Users
                .Where(u => u.Id == id && u.DeletedAt != null)
                .ExecuteUpdateAsync(u => u.SetProperty(x => x.DeletedAt, _ => null)) > 0;
        }

        public async Task<User?> GetUserByIdAsync(int id, bool includeDeleted = false, bool tracking = false)
        {
            IQueryable<User> query = _dbContext.Users;

            if (!tracking)
                query = query.AsNoTracking();

            if (!includeDeleted)
                query = query.Where(u => u.DeletedAt == null);

            return await query.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<List<User>> GetAllUsersIncludeRelationshipsAsync(bool includeDeleted = false, bool tracking = false)
        {
            IQueryable<User> query = _dbContext.Users;

            if (!tracking)
                query = query.AsNoTracking();

            if (!includeDeleted)
                query = query.Where(u => u.DeletedAt == null);

            query = query.Include(u => u.Roles)
                    .ThenInclude(r => r.Role)
                .Include(u => u.Title)
                .Include(u => u.Subordinates)
                    .ThenInclude(em => em.Employee)
                        .ThenInclude(u => u.Title);

            return await query.ToListAsync();
        }

        public async Task<List<User>> GetSubordinatesByUserIdsAsync(List<int> ids, bool includeDeleted = false, bool tracking = false)
        {
            IQueryable<User> query = _dbContext.Users;

            if (!tracking)
                query = query.AsNoTracking();

            if (!includeDeleted)
                query = query.Where(u => u.DeletedAt == null);

            query = query.Where(u => ids.Contains(u.Id))
                .Include(u => u.Subordinates)
                    .ThenInclude(em => em.Employee);

            return await query.ToListAsync();

        }

        public async Task<List<User>> GetManagersByUserIdsAsync(List<int> ids, bool includeDeleted = false, bool tracking = false)
        {
            IQueryable<User> query = _dbContext.Users;

            if (!tracking)
                query = query.AsNoTracking();

            if (!includeDeleted)
                query = query.Where(u => u.DeletedAt == null);

            query = query.Where(u => ids.Contains(u.Id))
                .Include(u => u.Managers)
                    .ThenInclude(em => em.Manager);

            return await query.ToListAsync();
        }

        public async Task<List<User>> GetAllAdminsAsync(string? lastName, string? email, bool includeDeleted = false, bool tracking = false)
        {
            var query = GetRecords(includeDeletedEntities: includeDeleted)
                .Where(u => u.Roles.Where(r => r.DeletedAt == null).Any(r => r.Role.Rolename == "Admin"));

            if (!tracking)
                query = query.AsNoTracking();

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

        public async Task<(List<User> Data, int TotalCount)> GetAllManagersFilteredAsync(string? lastName, string? email, QueryParams parameters, bool includeDeleted = false,
            bool tracking = false)
        {
            IQueryable<User> query = GetRecords(includeDeletedEntities: includeDeleted)
                                     .Include(u => u.Roles.Where(r => r.DeletedAt == null))
                                        .ThenInclude(u => u.Role)
                                     .Include(u => u.Subordinates.Where(s => s.DeletedAt == null))
                                        .ThenInclude(subordinates => subordinates.Employee)
                                            .ThenInclude(e => e.Title)
                                     .Where(u => u.Subordinates.Count > 0);

            if (!tracking)
                query = query.AsNoTracking();

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

        public async Task<(List<User>? Data, int TotalCount)> GetAllUsersWithReferencesFilteredAsync(string? lastName, string? email, string? department, string? jobTitle, string? globalSearch,
            QueryParams parameters, bool includeDeleted = false, bool tracking = false)
        {
            IQueryable<User> query = _dbContext.Users;

            if (!tracking)
                query = query.AsNoTracking();

            if (!includeDeleted)
                query = query.Where(u => u.DeletedAt == null);

            query = query.Include(u => u.Roles.Where(r => r.DeletedAt == null))
                    .ThenInclude(u => u.Role)
                .Include(u => u.Title)
                .Include(u => u.Department);

            if (!string.IsNullOrEmpty(globalSearch))
            {
                Console.WriteLine($"Applying global search for: '{globalSearch}'");

                query = query.Where(u =>
                    (u.FirstName != null && u.FirstName.Contains(globalSearch)) ||
                    (u.LastName != null && u.LastName.Contains(globalSearch)) ||
                    (u.Email != null && u.Email.Contains(globalSearch)) ||
                    (u.Title != null && u.Title.Name != null && u.Title.Name.Contains(globalSearch)) ||
                    (u.Title != null && u.Department != null && u.Department.Name != null && u.Department.Name.Contains(globalSearch))
                );
            }
            else
            {
                // filtering 
                if (!string.IsNullOrEmpty(lastName))
                {
                    query = query.Where(u => u.LastName.Contains(lastName));
                }
                if (!string.IsNullOrEmpty(email))
                {
                    query = query.Where(u => u.Email.Contains(email));
                }
                if (!string.IsNullOrEmpty(department))
                {
                    query = query.Where(u => u.Title != null &&
                                            u.Department != null &&
                                            u.Department.Name.Contains(department));
                }
                if (!string.IsNullOrEmpty(jobTitle))
                {
                    query = query.Where(u => u.Title != null &&
                                            u.Title.Name.Contains(jobTitle));
                }
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

        public async Task<(List<User> Data, int TotalCount)> GetAllUnassignedUsersFilteredAsync(QueryParams parameters, bool includeDeleted = false, bool tracking = false)
        {
            IQueryable<User> query = _dbContext.Users;

            if(!tracking)
                query = query.AsNoTracking();

            if (!includeDeleted)
                query = query.Where(u => u.DeletedAt == null);

            query = query.Include(u => u.Managers)
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
