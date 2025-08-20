using ManagementSimulator.Core.Dtos.Requests.Projects;
using ManagementSimulator.Core.Dtos.Responses;
using ManagementSimulator.Core.Dtos.Responses.PagedResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Services.Interfaces
{
    public interface IProjectService
    {
        Task<PagedResponseDto<ProjectResponseDto>> GetAllProjectsFilteredAsync(QueriedProjectRequestDto payload);
        Task<List<ProjectResponseDto>> GetAllProjectsAsync();
        Task<ProjectResponseDto?> GetProjectByIdAsync(int id);
        Task<ProjectWithUsersResponseDto?> GetProjectWithUsersAsync(int id);
        Task<ProjectResponseDto> AddProjectAsync(CreateProjectRequestDto request);
        Task<ProjectResponseDto?> UpdateProjectAsync(int id, UpdateProjectRequestDto request);
        Task<bool> DeleteProjectAsync(int id);
        Task<bool> RestoreProjectAsync(int id);
        Task<UserProjectResponseDto> AssignUserToProjectAsync(int projectId, AssignUserToProjectRequestDto request);
        Task<bool> RemoveUserFromProjectAsync(int projectId, int userId);
        Task<UserProjectResponseDto?> UpdateUserProjectAssignmentAsync(int projectId, int userId, AssignUserToProjectRequestDto request);
        Task<List<UserProjectResponseDto>> GetProjectUsersAsync(int projectId);
    }
}