using ManagementSimulator.Core.Dtos.Requests.LeaveRequestType;
using ManagementSimulator.Core.Dtos.Requests.LeaveRequestTypes;
using ManagementSimulator.Core.Dtos.Responses;
using ManagementSimulator.Core.Dtos.Responses.PagedResponse;
using ManagementSimulator.Core.Services.Interfaces;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Enums;
using ManagementSimulator.Database.Repositories.Intefaces;
using ManagementSimulator.Infrastructure.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Services
{
    public class LeaveRequestTypeService : ILeaveRequestTypeService
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
                Title = l.Title ?? string.Empty,
                Description = l.Description ?? string.Empty,
                MaxDays = l.MaxDays,
                IsPaid = l.IsPaid
            }).ToList();
        }

        public async Task<LeaveRequestTypeResponseDto?> GetLeaveRequestTypeByIdAsync(int id)
        {
            var leaveRequestType = await _leaveRequestTypeRepository.GetFirstOrDefaultAsync(id);

            if (leaveRequestType == null)
            {
                throw new EntryNotFoundException(nameof(Database.Entities.LeaveRequestType), id);
            }

            return new LeaveRequestTypeResponseDto
            {
                Id = leaveRequestType.Id,
                Title = leaveRequestType.Title ?? string.Empty,
                Description = leaveRequestType.Description ?? string.Empty,
                MaxDays = leaveRequestType.MaxDays,
                IsPaid = leaveRequestType.IsPaid
            };
        }

        public async Task<LeaveRequestTypeResponseDto?> UpdateLeaveRequestTypeAsync(int id, UpdateLeaveRequestTypeRequestDto dto)
        {
            var leaveRequestType = await _leaveRequestTypeRepository.GetFirstOrDefaultAsync(id);

            if (leaveRequestType == null)
            {
                throw new EntryNotFoundException(nameof(Database.Entities.LeaveRequestType), id);
            }

            if (dto.Title != null && dto.Title != string.Empty)
            {
                var existingWithSameTitle = await _leaveRequestTypeRepository.GetLeaveRequestTypesByTitleAsync(dto.Title);

                if (existingWithSameTitle != null && existingWithSameTitle.Id != id)
                {
                    throw new UniqueConstraintViolationException(nameof(Database.Entities.LeaveRequestType), nameof(Database.Entities.LeaveRequestType.Title));
                }
            }

            PatchHelper.PatchRequestToEntity.PatchFrom<UpdateLeaveRequestTypeRequestDto, Database.Entities.LeaveRequestType>(leaveRequestType, dto);

            await _leaveRequestTypeRepository.UpdateAsync(leaveRequestType);

            return new LeaveRequestTypeResponseDto
            {
                Id = leaveRequestType.Id,
                Title = leaveRequestType.Title ?? string.Empty,
                Description = leaveRequestType.Description ?? string.Empty,
                MaxDays = leaveRequestType.MaxDays,
                IsPaid = leaveRequestType.IsPaid
            };
        }

        public async Task<bool> DeleteLeaveRequestTypeAsync(int id)
        {
            var leaveRequestType = await _leaveRequestTypeRepository.GetFirstOrDefaultAsync(id);

            if (leaveRequestType == null)
            {
                throw new EntryNotFoundException(nameof(Database.Entities.LeaveRequestType), id);
            }

            return await _leaveRequestTypeRepository.DeleteAsync(leaveRequestType.Id);
        }

        public async Task<LeaveRequestTypeResponseDto> AddLeaveRequestTypeAsync(CreateLeaveRequestTypeRequestDto dto)
        {
            if (await _leaveRequestTypeRepository.GetLeaveRequestTypesByTitleAsync(dto.Title) != null)
            {
                throw new UniqueConstraintViolationException(nameof(Database.Entities.LeaveRequestType), nameof(Database.Entities.LeaveRequestType.Title));
            }

            var leaveRequestType = new Database.Entities.LeaveRequestType
            {
                Title = dto.Title ?? string.Empty,
                Description = dto.Description ?? string.Empty,
                MaxDays = dto.MaxDays,
                IsPaid = dto.IsPaid
            };

            await _leaveRequestTypeRepository.AddAsync(leaveRequestType);

            return new LeaveRequestTypeResponseDto
            {
                Id = leaveRequestType.Id,
                Title = leaveRequestType.Title ?? string.Empty,
                Description = leaveRequestType.Description ?? string.Empty,
                MaxDays = leaveRequestType.MaxDays,
                IsPaid = leaveRequestType.IsPaid
            };
        }

        public async Task<PagedResponseDto<LeaveRequestTypeResponseDto>> GetAllLeaveRequestTypesFilteredAsync(QueriedLeaveRequestTypeRequestDto payload)
        {
            var (result, totalCount) = await _leaveRequestTypeRepository.GetAllLeaveRequestTypesFilteredAsync(payload.Title, payload.PagedQueryParams.ToQueryParams());

            if (result == null || !result.Any())
                return new PagedResponseDto<LeaveRequestTypeResponseDto>
                {
                    Data = new List<LeaveRequestTypeResponseDto>(),
                    Page = payload.PagedQueryParams.Page ?? 1,
                    PageSize = payload.PagedQueryParams.PageSize ?? 1,
                    TotalPages = 0
                };

            return new PagedResponseDto<LeaveRequestTypeResponseDto>
            {
                Data = result.Select(lrt => new LeaveRequestTypeResponseDto
                {
                    Id = lrt.Id,
                    Title = lrt.Title,
                    Description = lrt.Description ?? string.Empty,
                    MaxDays = lrt.MaxDays,
                    IsPaid = lrt.IsPaid
                }),
                Page = payload.PagedQueryParams.Page ?? 1,
                PageSize = payload.PagedQueryParams.PageSize ?? 1,
                TotalPages = payload.PagedQueryParams.PageSize != null ?
                    (int)Math.Ceiling((double)totalCount / (int)payload.PagedQueryParams.PageSize) : 1
            };
        }
    }
}
