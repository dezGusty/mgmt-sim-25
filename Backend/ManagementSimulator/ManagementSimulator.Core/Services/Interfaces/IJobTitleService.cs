using ManagementSimulator.Core.Dtos.Requests.Departments;
using ManagementSimulator.Core.Dtos.Requests.JobTitle;
using ManagementSimulator.Core.Dtos.Requests.LeaveRequest;
using ManagementSimulator.Core.Dtos.Responses;
using ManagementSimulator.Core.Dtos.Responses.PagedResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Services.Interfaces
{
    public interface IJobTitleService
    {
        Task<PagedResponseDto<JobTitleResponseDto>> GetAllJobTitlesFilteredAsync(QueriedJobTitleRequestDto payload);
        Task<List<JobTitleResponseDto>> GetAllJobTitlesAsync();
        Task<JobTitleResponseDto?> GetJobTitleByIdAsync(int id);
        Task<JobTitleResponseDto> AddJobTitleAsync(CreateJobTitleRequestDto request);
        Task<JobTitleResponseDto?> UpdateJobTitleAsync(int id,UpdateJobTitleRequestDto request);
        Task<bool> DeleteJobTitleAsync(int id);
        Task<bool> RestoreJobTitleAsync(int id);    
    }
}
