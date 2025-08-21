using ManagementSimulator.Database.Context;
using ManagementSimulator.Database.Dtos.Project;
using ManagementSimulator.Database.Dtos.QueryParams;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Extensions;
using ManagementSimulator.Database.Repositories.Intefaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Database.Repositories
{
    public class ProjectRepository : BaseRepository<Project>, IProjectRepository
    {
        private readonly MGMTSimulatorDbContext _dbContext;

        public ProjectRepository(MGMTSimulatorDbContext databaseContext) : base(databaseContext)
        {
            _dbContext = databaseContext;
        }

        public async Task<List<Project>> GetAllProjectsAsync(List<int> ids, bool includeDeleted = false, bool tracking = false)
        {
            IQueryable<Project> query = _dbContext.Projects;

            if (!tracking)
                query = query.AsNoTracking();

            if (!includeDeleted)
                query = query.Where(p => p.DeletedAt == null);

            if (ids?.Any() == true)
                query = query.Where(p => ids.Contains(p.Id));

            return await query.ToListAsync();
        }

        public async Task<Project?> GetProjectByIdAsync(int id, bool includeDeleted = false, bool tracking = false)
        {
            IQueryable<Project> query = _dbContext.Projects;

            if (!tracking)
                query = query.AsNoTracking();

            if (!includeDeleted)
                query = query.Where(p => p.DeletedAt == null);

            return await query.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Project?> GetProjectWithUsersAsync(int id, bool includeDeleted = false, bool tracking = false)
        {
            IQueryable<Project> query = _dbContext.Projects
                .Include(p => p.UserProjects)
                .ThenInclude(up => up.User);

            if (!tracking)
                query = query.AsNoTracking();

            if (!includeDeleted)
                query = query.Where(p => p.DeletedAt == null);

            return await query.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Project?> GetProjectByNameAsync(string name, bool includeDeleted = false, bool tracking = false)
        {
            IQueryable<Project> query = _dbContext.Projects;

            if (!tracking)
                query = query.AsNoTracking();

            if (!includeDeleted)
                query = query.Where(p => p.DeletedAt == null);

            return await query.FirstOrDefaultAsync(p => p.Name == name);
        }

        public async Task<(List<ProjectDto> Data, int TotalCount)> GetAllProjectsFilteredAsync(
            string? projectName,
            bool? isActive,
            DateTime? startDateFrom,
            DateTime? startDateTo,
            DateTime? endDateFrom,
            DateTime? endDateTo,
            QueryParams parameters,
            bool includeDeleted = false,
            bool tracking = false)
        {
            IQueryable<Project> query = _dbContext.Projects
                .Include(p => p.UserProjects);

            if (!tracking)
                query = query.AsNoTracking();

            if (!includeDeleted)
                query = query.Where(p => p.DeletedAt == null);

            // Filtering
            if (!string.IsNullOrEmpty(projectName))
                query = query.Where(p => p.Name.Contains(projectName));

            if (isActive.HasValue)
                query = query.Where(p => p.IsActive == isActive.Value);

            if (startDateFrom.HasValue)
                query = query.Where(p => p.StartDate >= startDateFrom.Value);

            if (startDateTo.HasValue)
                query = query.Where(p => p.StartDate <= startDateTo.Value);

            if (endDateFrom.HasValue)
                query = query.Where(p => p.EndDate >= endDateFrom.Value);

            if (endDateTo.HasValue)
                query = query.Where(p => p.EndDate <= endDateTo.Value);

            var totalCount = await query.CountAsync();

            var selectQuery = query.Select(p => new ProjectDto
            {
                Id = p.Id,
                Name = p.Name,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                BudgetedFTEs = p.BudgetedFTEs,
                IsActive = p.IsActive,
                AssignedUsersCount = p.UserProjects.Count,
                TotalAssignedPercentage = p.UserProjects.Sum(up => up.TimePercentagePerProject),
                CreatedAt = p.CreatedAt,
                DeletedAt = p.DeletedAt,
                ModifiedAt = p.ModifiedAt
            });

            if (parameters == null)
                return (await selectQuery.ToListAsync(), totalCount);

            // Sorting
            if (string.IsNullOrEmpty(parameters.SortBy))
                query = query.OrderBy(p => p.Id);
            else
                query = query.ApplySorting<Project>(parameters.SortBy, parameters.SortDescending ?? false);

            // Pagination
            if (parameters.Page == null || parameters.Page <= 0 || parameters.PageSize == null || parameters.PageSize <= 0)
            {
                return (await selectQuery.ToListAsync(), totalCount);
            }
            else
            {
                var pagedData = await selectQuery
                    .Skip((parameters.Page.Value - 1) * parameters.PageSize.Value)
                    .Take(parameters.PageSize.Value)
                    .ToListAsync();

                return (pagedData, totalCount);
            }
        }

        public async Task<UserProject?> GetUserProjectAsync(int userId, int projectId)
        {
            return await _dbContext.UserProjects
                .AsNoTracking()
                .FirstOrDefaultAsync(up => up.UserId == userId && up.ProjectId == projectId);
        }

        public async Task<List<UserProject>> GetProjectUsersAsync(int projectId)
        {
            return await _dbContext.UserProjects
                .Include(up => up.User)
                .Where(up => up.ProjectId == projectId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<UserProject> AddUserToProjectAsync(UserProject userProject)
        {
            _dbContext.UserProjects.Add(userProject);
            await _dbContext.SaveChangesAsync();
            return userProject;
        }

        public async Task<bool> RemoveUserFromProjectAsync(int userId, int projectId)
        {
            var userProject = await _dbContext.UserProjects
                .FirstOrDefaultAsync(up => up.UserId == userId && up.ProjectId == projectId);

            if (userProject == null)
                return false;

            _dbContext.UserProjects.Remove(userProject);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateUserProjectAssignmentAsync(UserProject userProject)
        {
            var existing = await _dbContext.UserProjects
                .FirstOrDefaultAsync(up => up.UserId == userProject.UserId && up.ProjectId == userProject.ProjectId);

            if (existing == null)
                return false;

            existing.TimePercentagePerProject = userProject.TimePercentagePerProject;
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}