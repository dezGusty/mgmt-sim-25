using ManagementSimulator.Core.Dtos.Requests.LeaveRequestType;
using ManagementSimulator.Core.Dtos.Responses;
using ManagementSimulator.Core.Services.Interfaces;
using ManagementSimulator.Database.Repositories.Intefaces;
using ManagementSimulator.Database.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Services
{
    public class LeaveRequestTypeService: ILeaveRequestTypeService
    {
        private readonly ILeaveRequestTypeRepository _leaveRequestTypeRepository;

        public LeaveRequestTypeService(ILeaveRequestTypeRepository leaveRequestTypeRepository)
        {
            _leaveRequestTypeRepository = leaveRequestTypeRepository;
        }

        public async Task<List<LeaveRequestTypeResponseDto>> GetAllAsync()
        {
            var leaveRequestTypes = await _leaveRequestTypeRepository.GetAllAsync();
            return leaveRequestTypes.Select(l => new LeaveRequestTypeResponseDto
            {
                Id = l.Id,
                Description = l.Description ?? string.Empty
                // Other characteristics can be added here 
            }).ToList();
        }

        public async Task<LeaveRequestTypeResponseDto?> GetByIdAsync(int id)
        {
            var leaveRequestType = await _leaveRequestTypeRepository.GetFirstOrDefaultAsync(id);
            
            if (leaveRequestType == null) return null;
            return new LeaveRequestTypeResponseDto
            {
                Id = leaveRequestType.Id,
                Description = leaveRequestType.Description ?? string.Empty
                // Other characteristics can be added here 
            };
        }



        public async Task<LeaveRequestTypeResponseDto> AddAsync(CreateLeaveRequestTypeRequestDto dto)
        {
            var newLeaveRequestType = new LeaveRequestType
            {
                Description = dto.Description ?? string.Empty
            };

            await _leaveRequestTypeRepository.AddAsync(newLeaveRequestType);

            return new LeaveRequestTypeResponseDto
            {
                Id = newLeaveRequestType.Id,
                Description = newLeaveRequestType.Description
            };
        }


        public async Task<LeaveRequestTypeResponseDto?> UpdateAsync(int id,UpdateLeaveRequestTypeRequestDto dto)
        {
            var leaveRequestType = await _leaveRequestTypeRepository.GetFirstOrDefaultAsync(id);
            
            if (leaveRequestType == null) return null;
            leaveRequestType.Description = dto.Description;
            leaveRequestType.ModifiedAt = DateTime.UtcNow;

            var updatedLeaveRequestType = await _leaveRequestTypeRepository.UpdateAsync(leaveRequestType);
            if (updatedLeaveRequestType == null) return null;

            return new LeaveRequestTypeResponseDto
            {
                Id = updatedLeaveRequestType.Id,
                Description = updatedLeaveRequestType.Description ?? string.Empty,
                ModifiedAt = updatedLeaveRequestType.ModifiedAt,
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var leaveRequestType = await _leaveRequestTypeRepository.GetFirstOrDefaultAsync(id);
            if (leaveRequestType == null) return false;
            return await _leaveRequestTypeRepository.DeleteAsync(leaveRequestType.Id);
        }
    }
}
