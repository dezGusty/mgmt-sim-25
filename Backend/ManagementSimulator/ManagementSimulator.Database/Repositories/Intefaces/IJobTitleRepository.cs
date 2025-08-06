using ManagementSimulator.Database.Dtos.QueryParams;
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
        Task<JobTitle?> GetJobTitleByNameAsync(string name, bool includeDeleted = false, bool tracking = false);
        Task<List<JobTitle>> GetAllJobTitlesAsync(bool includeDeleted = false, bool tracking = false);
        Task<JobTitle?> GetJobTitleAsync(int id, bool includeDeleted = false, bool tracking = false);
        Task<List<JobTitle>> GetJobTitlesAsync(List<int> ids, bool includeDeleted = false, bool tracking = false);
        Task<(List<JobTitle> Data, int TotalCount)> GetAllJobTitlesFilteredAsync(string? jobTitleName, QueryParams parameters, bool includeDeleted = false, bool tracking = false);
        Task<(List<JobTitle> Data, int TotalCount)> GetAllInactiveJobTitlesFilteredAsync(string? jobTitleName, QueryParams parameters, bool tracking = false);
    }
}
