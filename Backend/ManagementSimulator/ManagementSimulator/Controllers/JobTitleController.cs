﻿using ManagementSimulator.Core.Dtos.Requests.JobTitle;
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
                return NotFound("No job titles found.");
            }
            return Ok(jobTitles);
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
                return NotFound($"Job title with ID {id} not found.");
            }
            return Ok(jobTitle);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddJobTitleAsync([FromBody] CreateJobTitleRequestDto dto)
        {
            var jobTitle = await _jobTitleService.AddJobTitleAsync(dto);
            return Created($"/api/jobtitles/{jobTitle.Id}", jobTitle);
        }

        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateJobTitleAsync(int id, [FromBody] UpdateJobTitleRequestDto dto)
        {
            var updatedJobTitle = await _jobTitleService.UpdateJobTitleAsync(id,dto);
            if (updatedJobTitle == null)
            {
                return NotFound($"Job title with ID {id} not found.");
            }
            return Ok(updatedJobTitle);
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
                return NotFound($"Job title with ID {id} not found.");
            }
            return Ok($"Job title with ID {id} deleted successfully.");
        }
    }
}