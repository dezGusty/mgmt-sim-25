using ManagementSimulator.Core.Dtos.Requests.JobTitle;
using ManagementSimulator.Core.Dtos.Responses;
using ManagementSimulator.Core.Services.Interfaces;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Repositories.Intefaces;


namespace ManagementSimulator.Core.Services
{
    public class JobTitleService: IJobTitleService
    {
        private readonly IJobTitleRepository _repository;

        public JobTitleService(IJobTitleRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<JobTitleResponseDto>> GetAllJobTitlesAsync()
        {
            var jobTitles = await _repository.GetAllAsync();
            return jobTitles.Select(j => new JobTitleResponseDto
            {
                Id = j.Id,
                Name = j.Name ?? string.Empty,
                DepartmentId = j.DepartmentId
            }).ToList();
        }

        public async Task<JobTitleResponseDto?> GetJobTitleByIdAsync(int id)
        {
            var jobTitle = await _repository.GetFirstOrDefaultAsync(id);
            if (jobTitle == null)
                return null;
            return new JobTitleResponseDto
            {
                Id = jobTitle.Id,
                Name = jobTitle.Name ?? string.Empty,
                DepartmentId = jobTitle.DepartmentId,
                DepartmentName = jobTitle.Department?.Name ?? string.Empty
            };
        }

        public async Task<JobTitleResponseDto> AddJobTitleAsync(CreateJobTitleRequestDto request)
        {
            var newJobTitle = new JobTitle
            {
                Name = request.Name,
                DepartmentId = request.DepartmentId
            };
            await _repository.AddAsync(newJobTitle);
            return new JobTitleResponseDto
            {
                Id = newJobTitle.Id,
                Name = newJobTitle.Name ?? string.Empty,
                DepartmentId = newJobTitle.DepartmentId,
                DepartmentName = newJobTitle.Department?.Name ?? string.Empty
            };
        }

        public async Task<JobTitleResponseDto?> UpdateJobTitleAsync(UpdateJobTitleRequestDto request)
        {
            var jobTitle = await _repository.GetFirstOrDefaultAsync(request.Id);
            if (jobTitle == null)
                return null;
            jobTitle.Name = request.Name;
            jobTitle.DepartmentId = request.DepartmentId;
            jobTitle.ModifiedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(jobTitle);
            return new JobTitleResponseDto
            {
                Id = jobTitle.Id,
                Name = jobTitle.Name ?? string.Empty,
                DepartmentId = jobTitle.DepartmentId,
                DepartmentName = jobTitle.Department?.Name ?? string.Empty
            };
        }

        public async Task<bool> DeleteJobTitleAsync(int id)
        {
            var jobTitle = await _repository.GetFirstOrDefaultAsync(id);
            if (jobTitle == null)
                return false;
            await _repository.DeleteAsync(jobTitle.Id);
            return true;
        }

    }
}
