using ManagementSimulator.Core.Dtos.Requests.LeaveRequest;
using ManagementSimulator.Core.Dtos.Responses;
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
        Task<LeaveRequestResponseDto> AddLeaveRequestAsync(LeaveRequestRequestDto dto);
        Task<List<LeaveRequestResponseDto>> GetRequestsByUserAsync(int userId);
        Task<List<LeaveRequestResponseDto>> GetAllRequestsAsync();
        Task<LeaveRequestResponseDto> GetRequestByIdAsync(int id);
        Task ReviewLeaveRequestAsync(ReviewLeaveRequestDto dto);
        Task<bool> DeleteLeaveRequestAsync(int id);
    }

}
