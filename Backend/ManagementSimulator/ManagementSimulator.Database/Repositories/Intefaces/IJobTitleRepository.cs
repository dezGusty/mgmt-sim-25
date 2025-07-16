using ManagementSimulator.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Database.Repositories.Intefaces
{
    public interface IJobTitleRepository: IBaseRepostory<JobTitle>
    {
        Task<List<JobTitle>> GetAllJobTitlesAsync();
        Task<JobTitle?> GetJobTitleByIdAsync(int id);
        Task<JobTitle?> AddJobTitleAsync(JobTitle jobTitle);
        Task<JobTitle?> UpdateJobTitleAsync(JobTitle jobTitle);
        Task<bool> DeleteJobTitleAsync(int id);
    }
}
