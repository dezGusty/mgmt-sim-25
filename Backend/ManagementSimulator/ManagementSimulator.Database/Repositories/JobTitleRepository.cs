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

        public async Task<JobTitle?> GetJobTitleByNameAsync(string name)
        {
            return await _dbContext.JobTitles.FirstOrDefaultAsync(jt => jt.Name == name);
        }

        public async Task<List<JobTitle>> GetAllJobTitlesWithDepartmentAsync()
        {
            return await _dbContext.JobTitles
                .Where(jt => jt.DeletedAt == null)
                .Include(jt => jt.Department)
                .Include(jt => jt.Users)
                .ToListAsync();
        }

        public async Task<JobTitle?> GetJobTitleWithDepartmentAsync(int id)
        {
            return await _dbContext.JobTitles
                .Include(jt => jt.Department)
                .FirstOrDefaultAsync(jt => jt.Id == id);
        }

        public async Task<List<JobTitle>?> GetJobTitlesWithDepartmentsAsync(List<int> ids)
        {
            return await _dbContext.JobTitles
                .Where(jt => jt.DeletedAt == null)
                .Where(jt => ids.Contains(jt.Id))
                .Include(jt => jt.Department)
                .ToListAsync();
        }

        public async Task<List<JobTitle>?> GetAllJobTitlesWithDepartmentsFilteredAsync(string? departmentName, string? jobTitleName, QueryParams parameters)
        {
            IQueryable<JobTitle> query = _dbContext.JobTitles
                                                    .Where(jt => jt.DeletedAt == null)
                                                    .Include(jt => jt.Department)
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

            if (parameters == null)
                return await query.ToListAsync();

            // sorting
            if (!parameters.SortBy.IsNullOrEmpty())
            {
                query = query.ApplySorting<JobTitle>(parameters.SortBy, parameters.SortDescending ?? false);
            }
            else
            {
                query = query.OrderBy(jt => jt.Id);
            }

            // Pagination
            if (parameters.Page == null || parameters.Page <= 0 || parameters.PageSize == null || parameters.PageSize <= 0)
            {
                return await query.ToListAsync();
            }
            else
            {
                return await query.Skip(((int)(parameters.Page) - 1) * (int)(parameters.PageSize))
                             .Take((int)parameters.PageSize)
                             .ToListAsync();
            }
        }
    }
}
