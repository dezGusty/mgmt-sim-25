using ManagementSimulator.Core.Dtos.Requests.LeaveRequestType;
using ManagementSimulator.Core.Dtos.Responses;
using ManagementSimulator.Core.Services.Interfaces;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Repositories.Intefaces;
using ManagementSimulator.Database.Entities;

using ManagementSimulator.Infrastructure.Exceptions;
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

        public async Task<List<LeaveRequestTypeResponseDto>> GetAllLeaveRequestTypesAsync()
        {
            var leaveRequestTypes = await _leaveRequestTypeRepository.GetAllAsync();
            return leaveRequestTypes.Select(l => new LeaveRequestTypeResponseDto
            {
                Id = l.Id,
                Description = l.Description ?? string.Empty
                // Other characteristics can be added here 
            }).ToList();
        }

        public async Task<LeaveRequestTypeResponseDto?> GetLeaveRequestTypeByIdAsync(int id)
        {
            var leaveRequestType = await _leaveRequestTypeRepository.GetFirstOrDefaultAsync(id);
            
            if (leaveRequestType == null)
            {
                throw new EntryNotFoundException(nameof(LeaveRequestType), id);
            }

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
            
            if (leaveRequestType == null)
            {
                throw new EntryNotFoundException(nameof(LeaveRequestType), dto.Id);
            }

            if(_leaveRequestTypeRepository.GetLeaveRequestTypesByDescriptionAsync(dto.Description) != null)
            {
                throw new UniqueConstraintViolationException(nameof(LeaveRequestType), nameof(LeaveRequestType.Description));
            }

            leaveRequestType.Description = dto.Description;
            leaveRequestType.ModifiedAt = DateTime.UtcNow;

            var updatedLeaveRequestType = await _leaveRequestTypeRepository.UpdateAsync(leaveRequestType);

            return new LeaveRequestTypeResponseDto
            {
                Id = updatedLeaveRequestType.Id,
                Description = updatedLeaveRequestType.Description ?? string.Empty,
                ModifiedAt = updatedLeaveRequestType.ModifiedAt,
            };
        }

        public async Task<bool> DeleteLeaveRequestTypeAsync(int id)
        {
            var leaveRequestType = await _leaveRequestTypeRepository.GetFirstOrDefaultAsync(id);

            if (leaveRequestType == null)
            { 
                throw new EntryNotFoundException(nameof(LeaveRequestType), id);
            }

            return await _leaveRequestTypeRepository.DeleteAsync(leaveRequestType.Id);
        }

        public async Task<LeaveRequestTypeResponseDto> AddLeaveRequestTypeAsync(CreateLeaveRequestTypeRequestDto dto)
        {
            if(await _leaveRequestTypeRepository.GetLeaveRequestTypesByDescriptionAsync(dto.Description) != null)
            {
                throw new UniqueConstraintViolationException(nameof(LeaveRequestType),nameof(LeaveRequestType.Description));
            }

            var leaveRequestType = new LeaveRequestType
            {
                Description = dto.Description,
            };

            await _leaveRequestTypeRepository.AddAsync(leaveRequestType);

            return new LeaveRequestTypeResponseDto
            {
                Id = leaveRequestType.Id,
                Description = leaveRequestType.Description ?? string.Empty,
            };
        }
    }
}
