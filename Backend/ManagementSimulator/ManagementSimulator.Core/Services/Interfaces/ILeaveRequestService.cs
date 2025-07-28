using ManagementSimulator.Core.Dtos.Requests.LeaveRequest;
using ManagementSimulator.Core.Dtos.Requests.LeaveRequests;
using ManagementSimulator.Core.Dtos.Responses.LeaveRequest;
using ManagementSimulator.Core.Dtos.Responses.PagedResponse;
using ManagementSimulator.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Services.Interfaces
{
    public interface ILeaveRequestService
    {
        Task<PagedResponseDto<LeaveRequestResponseDto>> GetAllLeaveRequestsFilteredAsync(int managerId,QueriedLeaveRequestRequestDto payload);
        Task<CreateLeaveRequestResponseDto> AddLeaveRequestAsync(CreateLeaveRequestRequestDto dto);
        Task<List<LeaveRequestResponseDto>> GetRequestsByUserAsync(int userId);
        Task<List<LeaveRequestResponseDto>> GetAllRequestsAsync();
        Task<LeaveRequestResponseDto> GetRequestByIdAsync(int id);
        Task ReviewLeaveRequestAsync(int id,ReviewLeaveRequestDto dto, int managerId);
        Task<LeaveRequestResponseDto> UpdateLeaveRequestAsync(int id, UpdateLeaveRequestDto dto);
        Task<bool> DeleteLeaveRequestAsync(int id);
        Task<List<LeaveRequestResponseDto>> GetLeaveRequestsForManagerAsync(int managerId);
    }

}
