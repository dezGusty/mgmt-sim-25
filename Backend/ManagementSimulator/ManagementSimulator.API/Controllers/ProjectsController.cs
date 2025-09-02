using ManagementSimulator.Core.Dtos.Requests.Projects;
using ManagementSimulator.Core.Services.Interfaces;
using ManagementSimulator.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManagementSimulator.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly ILogger<ProjectsController> _logger;

        public ProjectsController(IProjectService projectService, ILogger<ProjectsController> logger)
        {
            _projectService = projectService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProjects()
        {
            try
            {
                var projects = await _projectService.GetAllProjectsAsync();
                return Ok(projects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all projects");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("filtered")]
        public async Task<IActionResult> GetFilteredProjects([FromBody] QueriedProjectRequestDto request)
        {
            try
            {
                var projects = await _projectService.GetAllProjectsFilteredAsync(request);
                return Ok(projects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving filtered projects");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectById(int id)
        {
            try
            {
                var project = await _projectService.GetProjectByIdAsync(id);
                return Ok(project);
            }
            catch (EntryNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving project with id {ProjectId}", id);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("{id}/with-users")]
        public async Task<IActionResult> GetProjectWithUsers(int id)
        {
            try
            {
                var project = await _projectService.GetProjectWithUsersAsync(id);
                return Ok(project);
            }
            catch (EntryNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving project with users for id {ProjectId}", id);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequestDto request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Name))
                {
                    return BadRequest(new { error = "Project name is required" });
                }

                if (request.StartDate >= request.EndDate)
                {
                    return BadRequest(new { error = "End date must be after start date" });
                }

                if (request.BudgetedFTEs < 0)
                {
                    return BadRequest(new { error = "Budgeted FTEs cannot be negative" });
                }

                var project = await _projectService.AddProjectAsync(request);
                return CreatedAtAction(nameof(GetProjectById), new { id = project.Id }, project);
            }
            catch (UniqueConstraintViolationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating project");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, [FromBody] UpdateProjectRequestDto request)
        {
            try
            {
                if (request.StartDate.HasValue && request.EndDate.HasValue && request.StartDate >= request.EndDate)
                {
                    return BadRequest(new { error = "End date must be after start date" });
                }

                if (request.BudgetedFTEs.HasValue && request.BudgetedFTEs < 0)
                {
                    return BadRequest(new { error = "Budgeted FTEs cannot be negative" });
                }

                var project = await _projectService.UpdateProjectAsync(id, request);
                return Ok(project);
            }
            catch (EntryNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating project with id {ProjectId}", id);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            try
            {
                var success = await _projectService.DeleteProjectAsync(id);
                if (success)
                {
                    return Ok(new { message = "Project deleted successfully" });
                }
                return BadRequest(new { error = "Failed to delete project" });
            }
            catch (EntryNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting project with id {ProjectId}", id);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("{id}/restore")]
        public async Task<IActionResult> RestoreProject(int id)
        {
            try
            {
                var success = await _projectService.RestoreProjectAsync(id);
                if (success)
                {
                    return Ok(new { message = "Project restored successfully" });
                }
                return BadRequest(new { error = "Failed to restore project" });
            }
            catch (EntryNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring project with id {ProjectId}", id);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("{id}/users")]
        public async Task<IActionResult> GetProjectUsers(int id)
        {
            try
            {
                var users = await _projectService.GetProjectUsersAsync(id);
                return Ok(users);
            }
            catch (EntryNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users for project {ProjectId}", id);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPost("{id}/users")]
        public async Task<IActionResult> AssignUserToProject(int id, [FromBody] AssignUserToProjectRequestDto request)
        {
            try
            {
                if (request.UserId <= 0)
                {
                    return BadRequest(new { error = "Valid user ID is required" });
                }

                if (request.TimePercentagePerProject < 0 || request.TimePercentagePerProject > 100)
                {
                    return BadRequest(new { error = "Time percentage must be between 0 and 100" });
                }

                var assignment = await _projectService.AssignUserToProjectAsync(id, request);
                return CreatedAtAction(nameof(GetProjectUsers), new { id }, assignment);
            }
            catch (EntryNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (UniqueConstraintViolationException)
            {
                return Conflict(new { error = "User is already assigned to this project" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning user {UserId} to project {ProjectId}", request.UserId, id);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpDelete("{projectId}/users/{userId}")]
        public async Task<IActionResult> RemoveUserFromProject(int projectId, int userId)
        {
            try
            {
                var success = await _projectService.RemoveUserFromProjectAsync(projectId, userId);
                if (success)
                {
                    return Ok(new { message = "User removed from project successfully" });
                }
                return BadRequest(new { error = "Failed to remove user from project" });
            }
            catch (EntryNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing user {UserId} from project {ProjectId}", userId, projectId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpPut("{projectId}/users/{userId}")]
        public async Task<IActionResult> UpdateUserProjectAssignment(int projectId, int userId, [FromBody] AssignUserToProjectRequestDto request)
        {
            try
            {
                if (request.TimePercentagePerProject < 0 || request.TimePercentagePerProject > 100)
                {
                    return BadRequest(new { error = "Time percentage must be between 0 and 100" });
                }

                var assignment = await _projectService.UpdateUserProjectAssignmentAsync(projectId, userId, request);
                return Ok(assignment);
            }
            catch (EntryNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating assignment for user {UserId} in project {ProjectId}", userId, projectId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("{id}/available-users")]
        public async Task<IActionResult> GetAvailableUsersForProject(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string search = "")
        {
            try
            {
                var users = await _projectService.GetAvailableUsersForProjectAsync(id, page, pageSize, search);
                return Ok(users);
            }
            catch (EntryNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available users for project {ProjectId}", id);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }
}