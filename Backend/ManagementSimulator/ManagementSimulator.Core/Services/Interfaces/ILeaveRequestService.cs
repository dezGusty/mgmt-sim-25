using ManagementSimulator.Core.Dtos.Requests.LeaveRequest;
using ManagementSimulator.Core.Dtos.Requests.LeaveRequests;
using ManagementSimulator.Core.Dtos.Responses;
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
        Task<CreateLeaveRequestResponseDto> AddLeaveRequestByEmployeeAsync(CreateLeaveRequestByEmployeeDto dto, int userId);
        Task<List<LeaveRequestResponseDto>> GetRequestsByUserAsync(int userId);
        Task<(List<LeaveRequestResponseDto> Items, int TotalCount)> GetRequestsByUserPagedAsync(int userId, string? status = null, int pageSize = 10, int pageNumber = 1);
        Task<List<LeaveRequestResponseDto>> GetAllRequestsAsync();
        Task<LeaveRequestResponseDto> GetRequestByIdAsync(int id);
        Task ReviewLeaveRequestAsync(int id,ReviewLeaveRequestDto dto, int managerId);
        Task<LeaveRequestResponseDto> UpdateLeaveRequestAsync(int id, UpdateLeaveRequestDto dto);
        Task<bool> DeleteLeaveRequestAsync(int id);
        Task<List<LeaveRequestResponseDto>> GetLeaveRequestsForManagerAsync(int managerId, string? name = null);

        Task CancelLeaveRequestAsync(int requestId, int userId);

        Task<RemainingLeaveDaysResponseDto> GetRemainingLeaveDaysAsync(int userId, int leaveRequestTypeId, int year);
        Task<RemainingLeaveDaysResponseDto> GetRemainingLeaveDaysForPeriodAsync(int userId, int leaveRequestTypeId, DateTime startDate, DateTime endDate);
        Task<(List<LeaveRequestResponseDto> Items, int TotalCount)> GetFilteredLeaveRequestsAsync(int managerId, string status, int pageSize, int pageNumber);
    }
}
