using ManagementSimulator.Database.Dtos.Project;
using ManagementSimulator.Database.Dtos.QueryParams;
using ManagementSimulator.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Database.Repositories.Intefaces
{
    public interface IProjectRepository : IBaseRepostory<Project>
    {
        Task<List<Project>> GetAllProjectsAsync(List<int> ids, bool includeDeleted = false, bool tracking = false);
        Task<List<Project>> GetProjectsByIdsAsync(int[] projectIds, bool includeDeleted = false, bool tracking = false);
        Task<Project?> GetProjectByIdAsync(int id, bool includeDeleted = false, bool tracking = false);
        Task<Project?> GetProjectWithUsersAsync(int id, bool includeDeleted = false, bool tracking = false);
        Task<Project?> GetProjectByNameAsync(string name, bool includeDeleted = false, bool tracking = false);
        Task<(List<ProjectDto> Data, int TotalCount)> GetAllProjectsFilteredAsync(
            string? projectName,
            bool? isActive,
            DateTime? startDateFrom,
            DateTime? startDateTo,
            DateTime? endDateFrom,
            DateTime? endDateTo,
            QueryParams parameters,
            bool includeDeleted = false,
            bool tracking = false);
        Task<UserProject?> GetUserProjectAsync(int userId, int projectId);
        Task<List<UserProject>> GetProjectUsersAsync(int projectId);
        Task<List<UserProject>> GetUserProjectsByUserIdAsync(int userId);
        Task<UserProject> AddUserToProjectAsync(UserProject userProject);
        Task<bool> RemoveUserFromProjectAsync(int userId, int projectId);
        Task<bool> UpdateUserProjectAssignmentAsync(UserProject userProject);
    }
}