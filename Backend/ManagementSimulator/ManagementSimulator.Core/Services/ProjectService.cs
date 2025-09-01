using ManagementSimulator.Core.Dtos.Requests.Projects;
using ManagementSimulator.Core.Dtos.Responses;
using ManagementSimulator.Core.Dtos.Responses.PagedResponse;
using ManagementSimulator.Core.Services.Interfaces;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Repositories.Intefaces;
using ManagementSimulator.Infrastructure.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAvailabilityService _availabilityService;

        public ProjectService(IProjectRepository projectRepository, IUserRepository userRepository, IAvailabilityService availabilityService)
        {
            _projectRepository = projectRepository;
            _userRepository = userRepository;
            _availabilityService = availabilityService;
        }

        private async Task<(float totalFTEs, float remainingFTEs)> CalculateProjectFTEsAsync(int projectId, float budgetedFTEs)
        {
            var userProjects = await _projectRepository.GetProjectUsersAsync(projectId);
            float totalAssignedFTEs = 0f;

            foreach (var userProject in userProjects)
            {
                if (userProject.User != null)
                {
                    var userTotalAvailability = _availabilityService.CalculateTotalAvailability(userProject.User.EmploymentType);
                    var projectFTEAllocation = (userProject.TimePercentagePerProject / 100f) * userTotalAvailability;
                    totalAssignedFTEs += projectFTEAllocation;
                }
            }

            var remainingFTEs = budgetedFTEs - totalAssignedFTEs;
            return (totalAssignedFTEs, remainingFTEs > 0 ? remainingFTEs : 0);
        }

        public async Task<List<ProjectResponseDto>> GetAllProjectsAsync()
        {
            var projects = await _projectRepository.GetAllAsync();
            var result = new List<ProjectResponseDto>();

            foreach (var project in projects)
            {
                var (totalFTEs, remainingFTEs) = await CalculateProjectFTEsAsync(project.Id, project.BudgetedFTEs);

                var projectUtilizationPercentage = project.BudgetedFTEs > 0 ? (totalFTEs / project.BudgetedFTEs) * 100f : 0f;

                result.Add(new ProjectResponseDto
                {
                    Id = project.Id,
                    Name = project.Name,
                    StartDate = project.StartDate,
                    EndDate = project.EndDate,
                    BudgetedFTEs = project.BudgetedFTEs,
                    IsActive = project.IsActive,
                    AssignedUsersCount = project.UserProjects?.Count ?? 0,
                    TotalAssignedPercentage = projectUtilizationPercentage, // Changed to show project capacity utilization
                    TotalAssignedFTEs = totalFTEs,
                    RemainingFTEs = remainingFTEs,
                    CreatedAt = project.CreatedAt,
                    DeletedAt = project.DeletedAt,
                    ModifiedAt = project.ModifiedAt
                });
            }

            return result;
        }

        public async Task<ProjectResponseDto?> GetProjectByIdAsync(int id)
        {
            var project = await _projectRepository.GetProjectByIdAsync(id);
            if (project == null)
            {
                throw new EntryNotFoundException(nameof(Project), id);
            }

            var projectUsers = await _projectRepository.GetProjectUsersAsync(id);
            var (totalFTEs, remainingFTEs) = await CalculateProjectFTEsAsync(id, project.BudgetedFTEs);

            // Calculate project capacity utilization percentage
            var projectUtilizationPercentage = project.BudgetedFTEs > 0 ? (totalFTEs / project.BudgetedFTEs) * 100f : 0f;

            return new ProjectResponseDto
            {
                Id = project.Id,
                Name = project.Name,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                BudgetedFTEs = project.BudgetedFTEs,
                IsActive = project.IsActive,
                AssignedUsersCount = projectUsers.Count,
                TotalAssignedPercentage = projectUtilizationPercentage, // Changed to show project capacity utilization
                TotalAssignedFTEs = totalFTEs,
                RemainingFTEs = remainingFTEs,
                CreatedAt = project.CreatedAt,
                DeletedAt = project.DeletedAt,
                ModifiedAt = project.ModifiedAt
            };
        }

        public async Task<ProjectWithUsersResponseDto?> GetProjectWithUsersAsync(int id)
        {
            var project = await _projectRepository.GetProjectWithUsersAsync(id);
            if (project == null)
            {
                throw new EntryNotFoundException(nameof(Project), id);
            }

            return new ProjectWithUsersResponseDto
            {
                Id = project.Id,
                Name = project.Name,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                BudgetedFTEs = project.BudgetedFTEs,
                IsActive = project.IsActive,
                AssignedUsers = project.UserProjects.Select(up => new UserProjectResponseDto
                {
                    UserId = up.UserId,
                    UserName = up.User?.FullName,
                    UserEmail = up.User?.Email,
                    JobTitleName = up.User?.Title?.Name,
                    EmploymentType = up.User?.EmploymentType.ToString(),
                    ProjectId = up.ProjectId,
                    ProjectName = project.Name,
                    TimePercentagePerProject = up.TimePercentagePerProject
                }).ToList(),
                CreatedAt = project.CreatedAt,
                DeletedAt = project.DeletedAt,
                ModifiedAt = project.ModifiedAt
            };
        }

        public async Task<ProjectResponseDto> AddProjectAsync(CreateProjectRequestDto request)
        {
            if (await _projectRepository.GetProjectByNameAsync(request.Name!, includeDeleted: true, tracking: false) != null)
            {
                throw new UniqueConstraintViolationException(nameof(Project), nameof(Project.Name));
            }

            var newProject = new Project
            {
                Name = request.Name!,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                BudgetedFTEs = request.BudgetedFTEs,
                IsActive = request.IsActive
            };

            await _projectRepository.AddAsync(newProject);

            return new ProjectResponseDto
            {
                Id = newProject.Id,
                Name = newProject.Name,
                StartDate = newProject.StartDate,
                EndDate = newProject.EndDate,
                BudgetedFTEs = newProject.BudgetedFTEs,
                IsActive = newProject.IsActive,
                AssignedUsersCount = 0,
                TotalAssignedPercentage = 0,
                CreatedAt = newProject.CreatedAt,
                DeletedAt = newProject.DeletedAt,
                ModifiedAt = newProject.ModifiedAt
            };
        }

        public async Task<ProjectResponseDto?> UpdateProjectAsync(int id, UpdateProjectRequestDto request)
        {
            var existing = await _projectRepository.GetProjectByIdAsync(id, tracking: true);
            if (existing == null)
            {
                throw new EntryNotFoundException(nameof(Project), id);
            }

            PatchHelper.PatchRequestToEntity.PatchFrom<UpdateProjectRequestDto, Project>(existing, request);
            existing.ModifiedAt = DateTime.UtcNow;

            await _projectRepository.SaveChangesAsync();

            var projectUsers = await _projectRepository.GetProjectUsersAsync(id);
            var (totalFTEs, remainingFTEs) = await CalculateProjectFTEsAsync(id, existing.BudgetedFTEs);
            var projectUtilizationPercentage = existing.BudgetedFTEs > 0 ? (totalFTEs / existing.BudgetedFTEs * 100f) : 0f;

            return new ProjectResponseDto
            {
                Id = existing.Id,
                Name = existing.Name,
                StartDate = existing.StartDate,
                EndDate = existing.EndDate,
                BudgetedFTEs = existing.BudgetedFTEs,
                IsActive = existing.IsActive,
                AssignedUsersCount = projectUsers.Count,
                TotalAssignedPercentage = projectUtilizationPercentage,
                TotalAssignedFTEs = totalFTEs,
                RemainingFTEs = remainingFTEs,
                CreatedAt = existing.CreatedAt,
                DeletedAt = existing.DeletedAt,
                ModifiedAt = existing.ModifiedAt
            };
        }

        public async Task<bool> DeleteProjectAsync(int id)
        {
            if (await _projectRepository.GetProjectByIdAsync(id) == null)
            {
                throw new EntryNotFoundException(nameof(Project), id);
            }

            return await _projectRepository.DeleteAsync(id);
        }

        public async Task<bool> RestoreProjectAsync(int id)
        {
            var projectToRestore = await _projectRepository.GetProjectByIdAsync(id, includeDeleted: true, tracking: true);
            if (projectToRestore == null)
            {
                throw new EntryNotFoundException(nameof(Project), id);
            }

            projectToRestore.DeletedAt = null;
            await _projectRepository.SaveChangesAsync();
            return true;
        }

        public async Task<PagedResponseDto<ProjectResponseDto>> GetAllProjectsFilteredAsync(QueriedProjectRequestDto payload)
        {
            var (result, totalCount) = await _projectRepository.GetAllProjectsFilteredAsync(
                payload.Name,
                payload.IsActive,
                payload.StartDateFrom,
                payload.StartDateTo,
                payload.EndDateFrom,
                payload.EndDateTo,
                payload.PagedQueryParams.ToQueryParams()
            );

            if (result == null || !result.Any())
                return new PagedResponseDto<ProjectResponseDto>
                {
                    Data = new List<ProjectResponseDto>(),
                    Page = payload.PagedQueryParams.Page ?? 1,
                    PageSize = payload.PagedQueryParams.PageSize ?? 1,
                    TotalPages = 0
                };

            var projectResponses = new List<ProjectResponseDto>();
            foreach (var p in result)
            {
                var (totalFTEs, remainingFTEs) = await CalculateProjectFTEsAsync(p.Id, p.BudgetedFTEs);

                var projectUtilizationPercentage = p.BudgetedFTEs > 0 ? (totalFTEs / p.BudgetedFTEs) * 100f : 0f;

                projectResponses.Add(new ProjectResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    BudgetedFTEs = p.BudgetedFTEs,
                    IsActive = p.IsActive,
                    AssignedUsersCount = p.AssignedUsersCount,
                    TotalAssignedPercentage = projectUtilizationPercentage, // Changed to show project capacity utilization
                    TotalAssignedFTEs = totalFTEs,
                    RemainingFTEs = remainingFTEs,
                    CreatedAt = p.CreatedAt,
                    DeletedAt = p.DeletedAt,
                    ModifiedAt = p.ModifiedAt
                });
            }

            return new PagedResponseDto<ProjectResponseDto>
            {
                Data = projectResponses,
                Page = payload.PagedQueryParams.Page ?? 1,
                PageSize = payload.PagedQueryParams.PageSize ?? 1,
                TotalPages = payload.PagedQueryParams.PageSize != null ?
                    (int)Math.Ceiling((double)totalCount / (int)payload.PagedQueryParams.PageSize) : 1
            };
        }

        public async Task<UserProjectResponseDto> AssignUserToProjectAsync(int projectId, AssignUserToProjectRequestDto request)
        {
            var project = await _projectRepository.GetProjectByIdAsync(projectId);
            if (project == null)
            {
                throw new EntryNotFoundException(nameof(Project), projectId);
            }

            var user = await _userRepository.GetFirstOrDefaultAsync(request.UserId);
            if (user == null)
            {
                throw new EntryNotFoundException(nameof(User), request.UserId);
            }

            var existingAssignment = await _projectRepository.GetUserProjectAsync(request.UserId, projectId);
            if (existingAssignment != null)
            {
                throw new UniqueConstraintViolationException(nameof(UserProject), "UserId_ProjectId");
            }

            var isValidAssignment = await _availabilityService.ValidateProjectAssignmentAsync(
                request.UserId, request.TimePercentagePerProject);
            if (!isValidAssignment)
            {
                throw new InvalidOperationException($"Assignment would exceed user's available capacity. User has insufficient remaining availability for {request.TimePercentagePerProject}% allocation.");
            }

            var userProject = new UserProject
            {
                UserId = request.UserId,
                ProjectId = projectId,
                TimePercentagePerProject = request.TimePercentagePerProject
            };

            await _projectRepository.AddUserToProjectAsync(userProject);

            await _availabilityService.UpdateUserAvailabilityAsync(request.UserId);

            return new UserProjectResponseDto
            {
                UserId = user.Id,
                UserName = user.FullName,
                UserEmail = user.Email,
                ProjectId = project.Id,
                ProjectName = project.Name,
                TimePercentagePerProject = request.TimePercentagePerProject
            };
        }

        public async Task<bool> RemoveUserFromProjectAsync(int projectId, int userId)
        {
            var project = await _projectRepository.GetProjectByIdAsync(projectId);
            if (project == null)
            {
                throw new EntryNotFoundException(nameof(Project), projectId);
            }

            var user = await _userRepository.GetFirstOrDefaultAsync(userId);
            if (user == null)
            {
                throw new EntryNotFoundException(nameof(User), userId);
            }

            var result = await _projectRepository.RemoveUserFromProjectAsync(userId, projectId);

            if (result)
            {
                await _availabilityService.UpdateUserAvailabilityAsync(userId);
            }

            return result;
        }

        public async Task<UserProjectResponseDto?> UpdateUserProjectAssignmentAsync(int projectId, int userId, AssignUserToProjectRequestDto request)
        {
            var project = await _projectRepository.GetProjectByIdAsync(projectId);
            if (project == null)
            {
                throw new EntryNotFoundException(nameof(Project), projectId);
            }

            var user = await _userRepository.GetFirstOrDefaultAsync(userId);
            if (user == null)
            {
                throw new EntryNotFoundException(nameof(User), userId);
            }

            var isValidAssignment = await _availabilityService.ValidateProjectAssignmentAsync(
                userId, request.TimePercentagePerProject, projectId);
            if (!isValidAssignment)
            {
                throw new InvalidOperationException($"Updated assignment would exceed user's available capacity. User has insufficient remaining availability for {request.TimePercentagePerProject}% allocation.");
            }

            var userProject = new UserProject
            {
                UserId = userId,
                ProjectId = projectId,
                TimePercentagePerProject = request.TimePercentagePerProject
            };

            var success = await _projectRepository.UpdateUserProjectAssignmentAsync(userProject);
            if (!success)
            {
                throw new EntryNotFoundException(nameof(UserProject), $"UserId:{userId}, ProjectId:{projectId}");
            }

            await _availabilityService.UpdateUserAvailabilityAsync(userId);

            return new UserProjectResponseDto
            {
                UserId = user.Id,
                UserName = user.FullName,
                UserEmail = user.Email,
                ProjectId = project.Id,
                ProjectName = project.Name,
                TimePercentagePerProject = request.TimePercentagePerProject
            };
        }

        public async Task<List<UserProjectResponseDto>> GetProjectUsersAsync(int projectId)
        {
            var project = await _projectRepository.GetProjectByIdAsync(projectId);
            if (project == null)
            {
                throw new EntryNotFoundException(nameof(Project), projectId);
            }

            var projectUsers = await _projectRepository.GetProjectUsersAsync(projectId);

            return projectUsers.Select(up => new UserProjectResponseDto
            {
                UserId = up.UserId,
                UserName = up.User?.FullName,
                UserEmail = up.User?.Email,
                ProjectId = up.ProjectId,
                ProjectName = project.Name,
                TimePercentagePerProject = up.TimePercentagePerProject
            }).ToList();
        }
    }
}