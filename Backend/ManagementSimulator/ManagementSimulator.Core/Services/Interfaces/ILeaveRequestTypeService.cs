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
        Task<List<LeaveRequestTypeResponseDto>> GetAllAsync();
        Task<LeaveRequestTypeResponseDto?> GetByIdAsync(int id);
        Task<LeaveRequestTypeResponseDto> AddAsync(CreateLeaveRequestTypeRequestDto dto);
        Task<LeaveRequestTypeResponseDto?> UpdateAsync(int id, UpdateLeaveRequestTypeRequestDto dto);
        Task<bool> DeleteAsync(int id);

    }
}
