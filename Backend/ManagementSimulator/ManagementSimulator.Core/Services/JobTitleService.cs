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
            var jobTitles = await _jobTitleRepository.GetAllJobTitlesWithDepartmentAsync();
            return jobTitles.Select(j => new JobTitleResponseDto
            {
                Id = j.Id,
                Name = j.Name ?? string.Empty,
                DepartmentId = j.DepartmentId,
                DepartmentName = j.Department?.Name ?? string.Empty,
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
                DepartmentId = jobTitle.DepartmentId,
                DepartmentName = jobTitle.Department?.Name ?? string.Empty
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

            if(await _departmentRepository.GetFirstOrDefaultAsync(request.DepartmentId) == null)
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
            if (request.DepartmentId != null && await _departmentRepository.GetFirstOrDefaultAsync((int)request.DepartmentId) == null)
            {
                throw new EntryNotFoundException(nameof(Department), request.DepartmentId);
            }

            if (request.Name != null && request.Name != string.Empty)
            {
                var jt = await _jobTitleRepository.GetJobTitleByNameAsync(request.Name);
                if(jt != null)
                {
                    if (jt.DeletedAt != null)
                        jt.DeletedAt = null;
                    else
                        throw new UniqueConstraintViolationException(nameof(JobTitle), nameof(JobTitle.Name));
                }
                else
                    throw new EntryNotFoundException(nameof(JobTitle), nameof(JobTitle.Name));
            }

            var jobTitle = await _jobTitleRepository.GetJobTitleWithDepartmentAsync(id);
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

        public async Task<PagedResponseDto<JobTitleResponseDto>> GetAllJobTitlesFilteredAsync(QueriedJobTitleRequestDto payload)
        {
            var (result, totalCount) = await _jobTitleRepository.GetAllJobTitlesWithDepartmentsFilteredAsync(payload.DepartmentName, payload.JobTitleName, payload.PagedQueryParams.ToQueryParams());

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
                    DepartmentName = jt.Department.Name ?? string.Empty,
                    DepartmentId = jt.Department.Id,
                    EmployeeCount = jt.Users.Count,
                }),
                Page = payload.PagedQueryParams.Page ?? 1,
                PageSize = payload.PagedQueryParams.PageSize ?? 1,
                TotalPages = payload.PagedQueryParams.PageSize != null ?
                    (int)Math.Ceiling((double)totalCount / (int)payload.PagedQueryParams.PageSize) : 1 
            };
        }
    }
}
