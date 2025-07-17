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
            try
            {
                var jobTitles = await _jobTitleService.GetAllJobTitlesAsync();
                if (jobTitles == null || !jobTitles.Any())
                {
                    return NotFound("No job titles found.");
                }
                return Ok(jobTitles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving job titles");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving job titles.");
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetJobTitleByIdAsync(int id)
        {
            try
            {
                var jobTitle = await _jobTitleService.GetJobTitleByIdAsync(id);
                if (jobTitle == null)
                {
                    return NotFound($"Job title with ID {id} not found.");
                }
                return Ok(jobTitle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving job title by ID");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the job title.");
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddJobTitleAsync([FromBody] CreateJobTitleRequestDto dto)
        {
            try
            {
                var jobTitle = await _jobTitleService.AddJobTitleAsync(dto);
                return Created($"/api/jobtitles/{jobTitle.Id}", jobTitle);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating job title");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the job title.");
            }
        }

        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateJobTitleAsync(int id, [FromBody] UpdateJobTitleRequestDto dto)
        {
            try
            {
                var updatedJobTitle = await _jobTitleService.UpdateJobTitleAsync(id,dto);
                if (updatedJobTitle == null)
                {
                    return NotFound($"Job title with ID {id} not found.");
                }
                return Ok(updatedJobTitle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating job title");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the job title.");
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteJobTitleAsync(int id)
        {
            try
            {
                bool result = await _jobTitleService.DeleteJobTitleAsync(id);
                if (!result)
                {
                    return NotFound($"Job title with ID {id} not found.");
                }
                return Ok($"Job title with ID {id} deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting job title");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the job title.");
            }
        }
    }
}