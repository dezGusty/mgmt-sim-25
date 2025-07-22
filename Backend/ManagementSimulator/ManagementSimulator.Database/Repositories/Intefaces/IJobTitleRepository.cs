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
        Task<JobTitle?> GetJobTitleByNameAsync(string name);
        Task<List<JobTitle>> GetAllJobTitlesWithDepartmentAsync();
        Task<JobTitle?> GetJobTitleWithDepartmentAsync(int id); 
    }
}
