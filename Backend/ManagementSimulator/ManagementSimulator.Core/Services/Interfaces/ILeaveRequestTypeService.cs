using ManagementSimulator.Core.Dtos.Requests.LeaveRequestType;
using ManagementSimulator.Core.Dtos.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Services.Interfaces
{
    public interface ILeaveRequestTypeService
    {
        Task<List<LeaveRequestTypeResponseDto>> GetAllLeaveRequestTypesAsync();
        Task<LeaveRequestTypeResponseDto?> GetLeaveRequestTypeByIdAsync(int id);
        Task<LeaveRequestTypeResponseDto?> UpdateLeaveRequestTypeAsync(UpdateLeaveRequestTypeRequestDto dto);
        Task<bool> DeleteLeaveRequestTypeAsync(int id);
        Task<LeaveRequestTypeResponseDto> AddLeaveRequestTypeAsync(CreateLeaveRequestTypeRequestDto dto);
    }
}
