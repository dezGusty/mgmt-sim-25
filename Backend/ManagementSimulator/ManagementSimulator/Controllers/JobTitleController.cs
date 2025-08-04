using ManagementSimulator.Core.Dtos.Requests.JobTitle;
using ManagementSimulator.Core.Dtos.Responses;
using ManagementSimulator.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ManagementSimulator.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobTitlesController : ControllerBase
    {
        private readonly IJobTitleService _jobTitleService;
        private readonly ILogger<JobTitlesController> _logger;

        public JobTitlesController(IJobTitleService jobTitleService, ILogger<JobTitlesController> logger)
        {
            _jobTitleService = jobTitleService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllJobTitlesAsync()
        {
            var jobTitles = await _jobTitleService.GetAllJobTitlesAsync();

            if (jobTitles == null || !jobTitles.Any())
            {
                return NotFound(new
                {
                    Message = "No job titles found.",
                    Data = new List<JobTitleResponseDto>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Message = "Job titles retrieved successfully.",
                Data = jobTitles,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("queried")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllJobTitlesFilteredAsync([FromQuery] QueriedJobTitleRequestDto payload)
        {
            var jobTitles = await _jobTitleService.GetAllJobTitlesFilteredAsync(payload);

            if (jobTitles == null || jobTitles.Data == null || !jobTitles.Data.Any())
            {
                return NotFound(new
                {
                    Message = "No job titles found.",
                    Data = new List<JobTitleResponseDto>(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Message = "Job titles retrieved successfully.",
                Data = jobTitles,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetJobTitleByIdAsync(int id)
        {
            var jobTitle = await _jobTitleService.GetJobTitleByIdAsync(id);

            if (jobTitle == null)
            {
                return NotFound(new
                {
                    Message = $"Job title with Id {id} not found.",
                    Data = new JobTitleResponseDto(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Message = "Job title retrieved successfully.",
                Data = jobTitle,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddJobTitleAsync([FromBody] CreateJobTitleRequestDto dto)
        {
            var jobTitle = await _jobTitleService.AddJobTitleAsync(dto);

            return Created($"/api/jobtitles/{jobTitle.Id}", new
            {
                Message = "Job title created successfully.",
                Data = jobTitle,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateJobTitleAsync(int id, [FromBody] UpdateJobTitleRequestDto dto)
        {
            var updatedJobTitle = await _jobTitleService.UpdateJobTitleAsync(id, dto);

            if (updatedJobTitle == null)
            {
                return NotFound(new
                {
                    Message = $"Job title with Id {id} not found.",
                    Data = new JobTitleResponseDto(),
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Message = "Job title updated successfully.",
                Data = updatedJobTitle,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteJobTitleAsync(int id)
        {
            bool result = await _jobTitleService.DeleteJobTitleAsync(id);

            if (!result)
            {
                return NotFound(new
                {
                    Message = $"Job title with Id {id} not found.",
                    Data = false,
                    Success = false,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                Message = "Job title deleted successfully.",
                Data = result,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpPatch("restore/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RestoreJobTitleAsync(int id)
        {
            bool result = await _jobTitleService.RestoreJobTitleAsync(id);

            return Ok(new
            {
                Message = "Job title restored successfully.",
                Data = result,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}