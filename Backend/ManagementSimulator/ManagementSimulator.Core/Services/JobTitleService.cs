using ManagementSimulator.Core.Dtos.Requests.Departments;
using ManagementSimulator.Core.Dtos.Requests.JobTitle;
using ManagementSimulator.Core.Dtos.Responses;
using ManagementSimulator.Core.Dtos.Responses.PagedResponse;
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
            var jobTitles = await _jobTitleRepository.GetAllJobTitlesAsync();
            return jobTitles.Select(j => new JobTitleResponseDto
            {
                Id = j.Id,
                Name = j.Name ?? string.Empty,
                EmployeeCount = j.Users.Count(u => u.DeletedAt == null),
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
            };
        }

        public async Task<JobTitleResponseDto> AddJobTitleAsync(CreateJobTitleRequestDto request)
        {
            JobTitle? jt = await _jobTitleRepository.GetJobTitleByNameAsync(request.Name!, includeDeleted: true);
            if (jt != null)
            {
                if (jt.DeletedAt == null)
                { 
                    throw new UniqueConstraintViolationException(nameof(JobTitle),nameof(JobTitle.Name));
                }
            }

            var newJobTitle = new JobTitle
            {
                Name = request.Name,
            };

            await _jobTitleRepository.AddAsync(newJobTitle);
            
            return new JobTitleResponseDto
            {
                Id = newJobTitle.Id,
                Name = newJobTitle.Name ?? string.Empty,
            };
        }

        public async Task<JobTitleResponseDto?> UpdateJobTitleAsync(int id, UpdateJobTitleRequestDto request)
        {
            if (request.Name != null && request.Name != string.Empty)
            {
                var jt = await _jobTitleRepository.GetJobTitleByNameAsync(request.Name);
                if(jt != null)
                {
                    if(jt.Id != id)
                        throw new UniqueConstraintViolationException(nameof(JobTitle), nameof(JobTitle.Name));
                }
                else
                    throw new EntryNotFoundException(nameof(JobTitle), nameof(JobTitle.Name));
            }

            var jobTitle = await _jobTitleRepository.GetJobTitleAsync(id, tracking:true);
            if (jobTitle == null)
            {
                throw new EntryNotFoundException(nameof(JobTitle), id);
            }

            PatchHelper.PatchRequestToEntity.PatchFrom<UpdateJobTitleRequestDto, JobTitle>(jobTitle, request);
            jobTitle.ModifiedAt = DateTime.UtcNow;

            await _jobTitleRepository.UpdateAsync(jobTitle);
            return new JobTitleResponseDto
            {
                Id = jobTitle.Id,
                Name = jobTitle.Name ?? string.Empty,
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

        public async Task<PagedResponseDto<JobTitleResponseDto>> GetAllJobTitlesFilteredAsync(QueriedJobTitleRequestDto payload)
        {
            var (result, totalCount) = await _jobTitleRepository.GetAllJobTitlesFilteredAsync(payload.JobTitleName, payload.PagedQueryParams.ToQueryParams(),
                includeDeleted: payload.IncludeDeleted ?? false);

            if (result == null || !result.Any())
                return new PagedResponseDto<JobTitleResponseDto>
                {
                    Data = new List<JobTitleResponseDto>(),
                    Page = payload.PagedQueryParams.Page ?? 1,
                    PageSize = payload.PagedQueryParams.PageSize ?? 1,
                    TotalPages = 0
                };

            return new PagedResponseDto<JobTitleResponseDto>
            {
                Data = result.Select(jt => new JobTitleResponseDto
                {
                    Id = jt.Id,
                    Name = jt.Name,
                    EmployeeCount = jt.Users.Count,
                    DeletedAt = jt.DeletedAt,
                }),
                Page = payload.PagedQueryParams.Page ?? 1,
                PageSize = payload.PagedQueryParams.PageSize ?? 1,
                TotalPages = payload.PagedQueryParams.PageSize != null ?
                    (int)Math.Ceiling((double)totalCount / (int)payload.PagedQueryParams.PageSize) : 1 
            };
        }

        public async Task<bool> RestoreJobTitleAsync(int id)
        {
            JobTitle? jt = await _jobTitleRepository.GetJobTitleAsync(id, includeDeleted: true, tracking: true);
            if (jt == null)
            {
                throw new EntryNotFoundException(nameof(JobTitle), nameof(JobTitle.Id));
            }

            jt.DeletedAt = null;
            await _departmentRepository.SaveChangesAsync();
            return true;
        }
    }
}
