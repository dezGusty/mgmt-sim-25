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
        public JobTitleRepository(MGMTSimulatorDbContext databaseContext, IAuditService auditService) : base(databaseContext, auditService)
        {
            _dbContext = databaseContext;
        }

        public async Task<JobTitle?> GetJobTitleByNameAsync(string name, bool includeDeleted = false, bool tracking = false)
        {
            IQueryable<JobTitle?> query = _dbContext.JobTitles;

            if (!tracking)
                query = query.AsNoTracking();

            if (!includeDeleted)
                query = query.Where(jt => jt.DeletedAt == null);

            return await query.FirstOrDefaultAsync(jt => jt.Name == name);
        }

        public async Task<List<JobTitle>> GetAllJobTitlesAsync(bool includeDeleted = false, bool tracking = false)
        {
            IQueryable<JobTitle> query = _dbContext.JobTitles;

            if (!tracking)
                query = query.AsNoTracking();

            if (!includeDeleted)
                query = query.Where(jt => jt.DeletedAt == null);

            query = query.Include(jt => jt.Users);

            return await query.ToListAsync();
        }

        public async Task<JobTitle?> GetJobTitleAsync(int id, bool includeDeleted = false, bool tracking = false)
        {
            IQueryable<JobTitle> query = _dbContext.JobTitles;

            if (!tracking)
                query = query.AsNoTracking();

            if (!includeDeleted)
                query = query.Where(jt => jt.DeletedAt == null);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<JobTitle>> GetJobTitlesAsync(List<int> ids, bool includeDeleted = false, bool tracking = false)
        {
            IQueryable<JobTitle> query = _dbContext.JobTitles;

            if (!tracking)
                query = query.AsNoTracking();

            if (!includeDeleted)
                query = query.Where(jt => jt.DeletedAt == null);

            query = query.Where(jt => ids.Contains(jt.Id));

            return await query.ToListAsync();
        }

        public async Task<(List<JobTitle> Data, int TotalCount)> GetAllJobTitlesFilteredAsync(string? jobTitleName, QueryParams parameters,
            bool includeDeleted = false, bool tracking = false)
        {
            IQueryable<JobTitle> query = _dbContext.JobTitles;

            if (!tracking)
                query = query.AsNoTracking();

            if (!includeDeleted)
                query = query.Where(jt => jt.DeletedAt == null);

            query = query.Include(jt => jt.Users);

            // Filtering
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
                if (string.Equals(parameters.SortBy, "jobTitleName", StringComparison.OrdinalIgnoreCase))
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

        public async Task<(List<JobTitle> Data, int TotalCount)> GetAllInactiveJobTitlesFilteredAsync(string? jobTitleName, QueryParams parameters, bool tracking = false)
        {
            IQueryable<JobTitle> query = _dbContext.JobTitles;

            if (!tracking)
                query = query.AsNoTracking();

            query = query.Where(jt => jt.DeletedAt != null).Include(jt => jt.Users);

            // Filtering
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
                if (string.Equals(parameters.SortBy, "jobTitleName", StringComparison.OrdinalIgnoreCase))
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
