using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManagementSimulator.Database.Context;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Repositories.Intefaces;
using Microsoft.EntityFrameworkCore;

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
                .Include(jt => jt.Department)
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
    }
}
