using ManagementSimulator.Core.Dtos.Requests.LeaveRequestType;
using ManagementSimulator.Core.Dtos.Requests.LeaveRequestTypes;
using ManagementSimulator.Core.Dtos.Responses;
using ManagementSimulator.Core.Dtos.Responses.PagedResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Services.Interfaces
{
    public interface ILeaveRequestTypeService
    {
        Task<PagedResponseDto<LeaveRequestTypeResponseDto>> GetAllLeaveRequestTypesFilteredAsync(QueriedLeaveRequestTypeRequestDto payload);
        Task<List<LeaveRequestTypeResponseDto>> GetAllLeaveRequestTypesAsync();
        Task<LeaveRequestTypeResponseDto?> GetLeaveRequestTypeByIdAsync(int id);
        Task<LeaveRequestTypeResponseDto?> UpdateLeaveRequestTypeAsync(int id,UpdateLeaveRequestTypeRequestDto dto);
        Task<bool> DeleteLeaveRequestTypeAsync(int id);
        Task<LeaveRequestTypeResponseDto> AddLeaveRequestTypeAsync(CreateLeaveRequestTypeRequestDto dto);
    }
}
