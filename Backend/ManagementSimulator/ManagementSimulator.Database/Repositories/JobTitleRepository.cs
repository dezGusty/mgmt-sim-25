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

        private readonly MGMTSimulatorDbContext _databaseContext;
        public JobTitleRepository(MGMTSimulatorDbContext databaseContext) : base(databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public async Task<JobTitle?> AddJobTitleAsync(JobTitle jobTitle)
        {
            Insert(jobTitle);
            await SaveChangesAsync();
            return jobTitle;
        }

        public async Task<bool> DeleteJobTitleAsync(int id)
        {
            JobTitle? jobTitle = await GetJobTitleByIdAsync(id);
            
            if(jobTitle is null)
            {
                return false;
            }

            SoftDelete(jobTitle);
            return true;
        }

        public async Task<List<JobTitle>> GetAllJobTitlesAsync()
        {
            return await GetAllAsync(includeDeletedEntities: false);
        }

        public Task<JobTitle?> GetJobTitleByIdAsync(int id)
        {
            return _databaseContext.JobTitles
                .FirstOrDefaultAsync(jobTitle => jobTitle.Id == id);
        }

        public async Task<JobTitle?> UpdateJobTitleAsync(JobTitle jobTitle)
        {
            Update(jobTitle);
            await SaveChangesAsync();
            return jobTitle;
        }
    }
}
