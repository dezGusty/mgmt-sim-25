using ManagementSimulator.Core.Dtos.Requests.LeaveRequestType;
using ManagementSimulator.Core.Dtos.Requests.LeaveRequestTypes;
using ManagementSimulator.Core.Dtos.Responses;
using ManagementSimulator.Core.Dtos.Responses.PagedResponse;
using ManagementSimulator.Core.Services.Interfaces;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Repositories.Intefaces;
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
                Description = l.Description ?? string.Empty,
                AdditionalDetails = l.AdditionalDetails ?? string.Empty,
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
                Description = leaveRequestType.Description ?? string.Empty,
                AdditionalDetails = leaveRequestType.AdditionalDetails ?? string.Empty,
            };
        }

        public async Task<LeaveRequestTypeResponseDto?> UpdateLeaveRequestTypeAsync(int id,UpdateLeaveRequestTypeRequestDto dto)
        {
            var leaveRequestType = await _leaveRequestTypeRepository.GetFirstOrDefaultAsync(id);
            
            if (leaveRequestType == null)
            {
                throw new EntryNotFoundException(nameof(LeaveRequestType), id);
            }

            if(dto.Description != null && dto.Description != string.Empty && 
                await _leaveRequestTypeRepository.GetLeaveRequestTypesByDescriptionAsync(dto.Description) != null)
            {
                throw new UniqueConstraintViolationException(nameof(LeaveRequestType), nameof(LeaveRequestType.Description));
            }

            PatchHelper.PatchRequestToEntity.PatchFrom<UpdateLeaveRequestTypeRequestDto, LeaveRequestType>(leaveRequestType, dto);
            leaveRequestType.ModifiedAt = DateTime.UtcNow;

            await _leaveRequestTypeRepository.SaveChangesAsync();

            return new LeaveRequestTypeResponseDto
            {
                Id = leaveRequestType.Id,
                Description = leaveRequestType.Description ?? string.Empty,
                AdditionalDetails = leaveRequestType.AdditionalDetails ?? string.Empty
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
                AdditionalDetails = leaveRequestType.AdditionalDetails ?? string.Empty
            };
        }

        public async Task<PagedResponseDto<LeaveRequestTypeResponseDto>> GetAllLeaveRequestTypesFilteredAsync(QueriedLeaveRequestTypeRequestDto payload)
        {
            var result = await _leaveRequestTypeRepository.GetAllLeaveRequestTypesFilteredAsync(payload.Description, payload.PagedQueryParams.ToQueryParams());

            return new PagedResponseDto<LeaveRequestTypeResponseDto>
            {
                Data = result.Select(lrt => new LeaveRequestTypeResponseDto
                {
                    Id = lrt.Id,
                    Description = lrt.Description,
                    AdditionalDetails = lrt.AdditionalDetails ?? string.Empty,
                }),
                Page = payload.PagedQueryParams.Page ?? 1,
                PageSize = payload.PagedQueryParams.PageSize ?? 1,
                TotalPages = payload.PagedQueryParams.PageSize != null ?
                    (int)Math.Ceiling((double)result.Count() / (int)payload.PagedQueryParams.PageSize) : 1
            };
        }
    }
}
