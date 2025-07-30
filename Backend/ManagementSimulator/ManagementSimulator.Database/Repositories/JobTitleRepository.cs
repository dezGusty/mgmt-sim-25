using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManagementSimulator.Database.Context;
using ManagementSimulator.Database.Dtos.QueryParams;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Extensions;
using ManagementSimulator.Database.Repositories.Intefaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ManagementSimulator.Database.Repositories
{
    public class JobTitleRepository : BaseRepository<JobTitle>, IJobTitleRepository
    {

        private readonly MGMTSimulatorDbContext _dbContext;
        public JobTitleRepository(MGMTSimulatorDbContext databaseContext) : base(databaseContext)
        {
            _dbContext = databaseContext;
        }

        public async Task<JobTitle?> GetJobTitleByNameAsync(string name, bool includeDeleted = false)
        {
            IQueryable<JobTitle?> query = _dbContext.JobTitles;
            if (!includeDeleted)
                query = query.Where(jt => jt.DeletedAt == null);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<JobTitle>?> GetAllJobTitlesWithDepartmentAsync(bool includeDeleted = false)
        {
            IQueryable<JobTitle> query = _dbContext.JobTitles;
            if (!includeDeleted)
                query = query.Where(jt => jt.DeletedAt == null);

            query = query.Include(jt => jt.Department)
                .Include(jt => jt.Users);

            return await query.ToListAsync();
        }

        public async Task<JobTitle?> GetJobTitleWithDepartmentAsync(int id, bool includeDeleted = false)
        {
            IQueryable<JobTitle> query = _dbContext.JobTitles;
            if (!includeDeleted)
                query = query.Where(jt => jt.DeletedAt == null);
            
            query = query.Include(jt => jt.Department);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<JobTitle>?> GetJobTitlesWithDepartmentsAsync(List<int> ids, bool includeDeleted = false)
        {
            IQueryable<JobTitle> query = _dbContext.JobTitles;
            if (!includeDeleted)
                query = query.Where(jt => jt.DeletedAt == null);

            query = query.Where(jt => ids.Contains(jt.Id))
                .Include(jt => jt.Department);

            return await query.ToListAsync();
        }

        public async Task<(List<JobTitle>? Data, int TotalCount)> GetAllJobTitlesWithDepartmentsFilteredAsync(string? departmentName, string? jobTitleName, QueryParams parameters,
            bool includeDeleted = false)
        {
            IQueryable<JobTitle> query = _dbContext.JobTitles;
            if (!includeDeleted)
                query = query.Where(jt => jt.DeletedAt == null);

            query = query.Include(jt => jt.Department)
                         .Include(jt => jt.Users);

            // Filtering 
            if (!string.IsNullOrEmpty(departmentName))
            {
                query = query.Where(jt => jt.Department != null && jt.Department.Name.Contains(departmentName));
            }
            if (!string.IsNullOrEmpty(jobTitleName))
            {
                query = query.Where(jt => jt.Name != null && jt.Name.Contains(jobTitleName));
            }

            var totalCount = await query.CountAsync();

            if (parameters == null)
                return (await query.ToListAsync(), totalCount);

            // sorting
            if (!string.IsNullOrEmpty(parameters.SortBy))
            {
                if (string.Equals(parameters.SortBy, "departmentName", StringComparison.OrdinalIgnoreCase))
                    query = query.OrderBy(jt => jt.Department.Name);
                else if (string.Equals(parameters.SortBy, "jobTitleName", StringComparison.OrdinalIgnoreCase))
                    query = query.OrderBy(jt => jt.Name);
            }
            else
            {
                query = query.OrderBy(jt => jt.Id);
            }

            // Pagination
            if (parameters.Page == null || parameters.Page <= 0 || parameters.PageSize == null || parameters.PageSize <= 0)
            {
                return (await query.ToListAsync(), totalCount);
            }
            else
            {
                var pagedData = await query.Skip(((int)(parameters.Page) - 1) * (int)(parameters.PageSize))
                             .Take((int)parameters.PageSize)
                             .ToListAsync();
                return (pagedData, totalCount);
            }
        }
    }
}
