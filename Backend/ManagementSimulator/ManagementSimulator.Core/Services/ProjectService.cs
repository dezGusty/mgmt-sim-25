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

        public ProjectService(IProjectRepository projectRepository, IUserRepository userRepository)
        {
            _projectRepository = projectRepository;
            _userRepository = userRepository;
        }

        public async Task<List<ProjectResponseDto>> GetAllProjectsAsync()
        {
            var projects = await _projectRepository.GetAllAsync();

            return projects.Select(p => new ProjectResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                BudgetedFTEs = p.BudgetedFTEs,
                IsActive = p.IsActive,
                AssignedUsersCount = p.UserProjects?.Count ?? 0,
                TotalAssignedPercentage = p.UserProjects?.Sum(up => up.TimePercentagePerProject) ?? 0,
                CreatedAt = p.CreatedAt,
                DeletedAt = p.DeletedAt,
                ModifiedAt = p.ModifiedAt
            }).ToList();
        }

        public async Task<ProjectResponseDto?> GetProjectByIdAsync(int id)
        {
            var project = await _projectRepository.GetProjectByIdAsync(id);
            if (project == null)
            {
                throw new EntryNotFoundException(nameof(Project), id);
            }

            var projectUsers = await _projectRepository.GetProjectUsersAsync(id);

            return new ProjectResponseDto
            {
                Id = project.Id,
                Name = project.Name,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                BudgetedFTEs = project.BudgetedFTEs,
                IsActive = project.IsActive,
                AssignedUsersCount = projectUsers.Count,
                TotalAssignedPercentage = projectUsers.Sum(up => up.TimePercentagePerProject),
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

            return new ProjectResponseDto
            {
                Id = existing.Id,
                Name = existing.Name,
                StartDate = existing.StartDate,
                EndDate = existing.EndDate,
                BudgetedFTEs = existing.BudgetedFTEs,
                IsActive = existing.IsActive,
                AssignedUsersCount = projectUsers.Count,
                TotalAssignedPercentage = projectUsers.Sum(up => up.TimePercentagePerProject),
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

            return new PagedResponseDto<ProjectResponseDto>
            {
                Data = result.Select(p => new ProjectResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    BudgetedFTEs = p.BudgetedFTEs,
                    IsActive = p.IsActive,
                    AssignedUsersCount = p.AssignedUsersCount,
                    TotalAssignedPercentage = p.TotalAssignedPercentage,
                    CreatedAt = p.CreatedAt,
                    DeletedAt = p.DeletedAt,
                    ModifiedAt = p.ModifiedAt
                }).ToList(),
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

            var userProject = new UserProject
            {
                UserId = request.UserId,
                ProjectId = projectId,
                TimePercentagePerProject = request.TimePercentagePerProject
            };

            await _projectRepository.AddUserToProjectAsync(userProject);

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

            return await _projectRepository.RemoveUserFromProjectAsync(userId, projectId);
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