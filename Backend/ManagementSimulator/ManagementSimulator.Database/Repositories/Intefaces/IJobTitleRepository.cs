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
        Task<JobTitle?> GetJobTitleByNameAsync(string name, bool includeDeleted = false);
        Task<List<JobTitle>> GetAllJobTitlesWithDepartmentAsync(bool includeDeleted = false);
        Task<JobTitle?> GetJobTitleWithDepartmentAsync(int id, bool includeDeleted = false);
        Task<List<JobTitle>?> GetJobTitlesWithDepartmentsAsync(List<int> ids, bool includeDeleted = false);
        Task<(List<JobTitle>? Data, int TotalCount)> GetAllJobTitlesWithDepartmentsFilteredAsync(string? departmentName,string? jobTitleName, QueryParams parameters, bool includeDeleted = false);
    }
}
