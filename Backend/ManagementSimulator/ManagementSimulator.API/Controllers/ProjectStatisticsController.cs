using ManagementSimulator.Core.Dtos.Requests.ProjectStatistics;
using ManagementSimulator.Core.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ManagementSimulator.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProjectStatisticsController : ControllerBase
    {
        private readonly IProjectStatisticsService _projectStatisticsService;
        private readonly ILogger<ProjectStatisticsController> _logger;

        public ProjectStatisticsController(
            IProjectStatisticsService projectStatisticsService,
            ILogger<ProjectStatisticsController> logger)
        {
            _projectStatisticsService = projectStatisticsService;
            _logger = logger;
        }

        [HttpPost("project")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProjectStatistics([FromBody] ProjectStatisticsRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var statistics = await _projectStatisticsService.GetProjectStatisticsAsync(request);
                return Ok(new
                {
                    Message = "Project statistics retrieved successfully.",
                    Data = statistics,
                    Success = true,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for project statistics: {ProjectId}", request.ProjectId);
                return NotFound(new
                {
                    Message = ex.Message,
                    Data = (object?)null,
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving project statistics for project {ProjectId}", request.ProjectId);
                return StatusCode(500, new
                {
                    Message = "An error occurred while retrieving project statistics.",
                    Data = (object?)null,
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpPost("overview")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProjectStatisticsOverview([FromBody] ProjectStatisticsOverviewRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var overview = await _projectStatisticsService.GetProjectStatisticsOverviewAsync(request);
                return Ok(new
                {
                    Message = "Project statistics overview retrieved successfully.",
                    Data = overview,
                    Success = true,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving project statistics overview");
                return StatusCode(500, new
                {
                    Message = "An error occurred while retrieving project statistics overview.",
                    Data = (object?)null,
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpPost("allocations/chart")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProjectAllocationChart([FromBody] ProjectAllocationChartRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var chartData = await _projectStatisticsService.GetProjectAllocationChartDataAsync(request);
                return Ok(new
                {
                    Message = "Project allocation chart data retrieved successfully.",
                    Data = chartData,
                    Success = true,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for project allocation chart: {ProjectId}, {Month}", request.ProjectId, request.Month);
                return NotFound(new
                {
                    Message = ex.Message,
                    Data = (object?)null,
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving project allocation chart for project {ProjectId}, month {Month}", request.ProjectId, request.Month);
                return StatusCode(500, new
                {
                    Message = "An error occurred while retrieving project allocation chart data.",
                    Data = (object?)null,
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpPost("budget/chart")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProjectBudgetChart([FromBody] ProjectBudgetChartRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var chartData = await _projectStatisticsService.GetProjectBudgetChartDataAsync(request);
                return Ok(new
                {
                    Message = "Project budget chart data retrieved successfully.",
                    Data = chartData,
                    Success = true,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for project budget chart: {ProjectId}, {Month}", request.ProjectId, request.Month);
                return NotFound(new
                {
                    Message = ex.Message,
                    Data = (object?)null,
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving project budget chart for project {ProjectId}, month {Month}", request.ProjectId, request.Month);
                return StatusCode(500, new
                {
                    Message = "An error occurred while retrieving project budget chart data.",
                    Data = (object?)null,
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpGet("fiscal-year/current")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCurrentFiscalYear()
        {
            try
            {
                var fiscalYear = await _projectStatisticsService.GetCurrentFiscalYearAsync();
                return Ok(new
                {
                    Message = "Current fiscal year retrieved successfully.",
                    Data = fiscalYear,
                    Success = true,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current fiscal year");
                return StatusCode(500, new
                {
                    Message = "An error occurred while retrieving current fiscal year.",
                    Data = (object?)null,
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpGet("fiscal-year/{year}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFiscalYear(int year)
        {
            try
            {
                if (year < 2000 || year > 2100)
                {
                    return BadRequest(new
                    {
                        Message = "Invalid fiscal year. Must be between 2000 and 2100.",
                        Data = (object?)null,
                        Success = false,
                        Timestamp = DateTime.UtcNow
                    });
                }

                var fiscalYear = await _projectStatisticsService.GetFiscalYearAsync(year);
                return Ok(new
                {
                    Message = "Fiscal year retrieved successfully.",
                    Data = fiscalYear,
                    Success = true,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving fiscal year {Year}", year);
                return StatusCode(500, new
                {
                    Message = "An error occurred while retrieving fiscal year.",
                    Data = (object?)null,
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpGet("project/{projectId}/available-months")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAvailableMonths(int projectId, [FromQuery] int? fiscalYear = null)
        {
            try
            {
                if (projectId <= 0)
                {
                    return BadRequest(new
                    {
                        Message = "Invalid project ID.",
                        Data = (object?)null,
                        Success = false,
                        Timestamp = DateTime.UtcNow
                    });
                }

                var currentFiscalYear = fiscalYear ?? (await _projectStatisticsService.GetCurrentFiscalYearAsync()).Year;
                var availableMonths = await _projectStatisticsService.GetAvailableMonthsForProjectAsync(projectId, currentFiscalYear);

                return Ok(new
                {
                    Message = "Available months retrieved successfully.",
                    Data = availableMonths,
                    Success = true,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available months for project {ProjectId}, fiscal year {FiscalYear}", projectId, fiscalYear);
                return StatusCode(500, new
                {
                    Message = "An error occurred while retrieving available months.",
                    Data = (object?)null,
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }
        }
    }
}