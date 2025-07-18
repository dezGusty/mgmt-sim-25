using ManagementSimulator.Core.Dtos.Requests.JobTitle;
using ManagementSimulator.Core.Dtos.Responses;
using ManagementSimulator.Core.Services.Interfaces;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Repositories;
using ManagementSimulator.Database.Repositories.Intefaces;
using ManagementSimulator.Infrastructure.Exceptions;


namespace ManagementSimulator.Core.Services
{
    public class JobTitleService: IJobTitleService
    {
        private readonly IJobTitleRepository _jobTitleRepository;
        private readonly IDeparmentRepository _departmentRepository;

        public JobTitleService(IJobTitleRepository jobTitleRepository, IDeparmentRepository deparmentRepository)
        {
            _jobTitleRepository = jobTitleRepository;
            _departmentRepository = deparmentRepository;
        }

        public async Task<List<JobTitleResponseDto>> GetAllJobTitlesAsync()
        {
            var jobTitles = await _jobTitleRepository.GetAllAsync();
            return jobTitles.Select(j => new JobTitleResponseDto
            {
                Id = j.Id,
                Name = j.Name ?? string.Empty,
                DepartmentId = j.DepartmentId
            }).ToList();
        }

        public async Task<JobTitleResponseDto?> GetJobTitleByIdAsync(int id)
        {
            var jobTitle = await _jobTitleRepository.GetFirstOrDefaultAsync(id);
            if (jobTitle == null)
            {
                throw new EntryNotFoundException(nameof(JobTitle), id);
            }

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
            if (_jobTitleRepository.GetJobTitleByNameAsync(request.Name!) != null)
            {
                throw new UniqueConstraintViolationException(nameof(JobTitle), nameof(JobTitle.Name));
            }

            if(_departmentRepository.GetFirstOrDefaultAsync(request.DepartmentId) == null)
            {
                throw new EntryNotFoundException(nameof(Department), request.DepartmentId);
            }

            var newJobTitle = new JobTitle
            {
                Name = request.Name,
                DepartmentId = request.DepartmentId
            };

            await _jobTitleRepository.AddAsync(newJobTitle);
            
            return new JobTitleResponseDto
            {
                Id = newJobTitle.Id,
                Name = newJobTitle.Name ?? string.Empty,
                DepartmentId = newJobTitle.DepartmentId,
                DepartmentName = newJobTitle.Department?.Name ?? string.Empty
            };
        }

        public async Task<JobTitleResponseDto?> UpdateJobTitleAsync(int id, UpdateJobTitleRequestDto request)
        {
            var jobTitle = await _jobTitleRepository.GetFirstOrDefaultAsync(request.Id);
            if (jobTitle == null)
            { 
                throw new EntryNotFoundException(nameof(JobTitle), request.Id); 
            }

            if(await _departmentRepository.GetFirstOrDefaultAsync(request.DepartmentId) == null)
            {
                throw new EntryNotFoundException(nameof(Department), request.DepartmentId);
            }

            if (await _jobTitleRepository.GetJobTitleByNameAsync(request.Name!) != null)
            {
                throw new UniqueConstraintViolationException(nameof(JobTitle), nameof(JobTitle.Name));
            }

            jobTitle.Name = request.Name;
            jobTitle.DepartmentId = request.DepartmentId;
            jobTitle.ModifiedAt = DateTime.UtcNow;

            await _jobTitleRepository.UpdateAsync(jobTitle);
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
            var jobTitle = await _jobTitleRepository.GetFirstOrDefaultAsync(id);

            if (jobTitle == null)
            {
                throw new EntryNotFoundException(nameof(JobTitle), id);
            }    

            await _jobTitleRepository.DeleteAsync(jobTitle.Id);
            return true;
        }

    }
}
