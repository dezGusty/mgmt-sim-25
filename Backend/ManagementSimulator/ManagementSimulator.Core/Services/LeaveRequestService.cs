using ManagementSimulator.Core.Dtos.Requests.LeaveRequest;
using ManagementSimulator.Core.Dtos.Requests.LeaveRequests;
using ManagementSimulator.Core.Dtos.Responses.LeaveRequest;
using ManagementSimulator.Core.Dtos.Responses.PagedResponse;
using ManagementSimulator.Core.Mapping;
using ManagementSimulator.Core.Services.Interfaces;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Enums;
using ManagementSimulator.Database.Repositories.Intefaces;
using ManagementSimulator.Infrastructure.Exceptions;


namespace ManagementSimulator.Core.Services
{
    public class LeaveRequestService : ILeaveRequestService
    {
        private readonly ILeaveRequestRepository _leaveRequestRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILeaveRequestTypeRepository _leaveRequestTypeRepository;

        public LeaveRequestService(ILeaveRequestRepository leaveRequestRepository,
                                   IUserRepository userRepository,
                                   ILeaveRequestTypeRepository leaveRequestTypeRepository)
        {
            _leaveRequestRepository = leaveRequestRepository;
            _userRepository = userRepository;
            _leaveRequestTypeRepository = leaveRequestTypeRepository;
        }

        public async Task<CreateLeaveRequestResponseDto> AddLeaveRequestAsync(CreateLeaveRequestRequestDto dto)
        {
            if (await _userRepository.GetFirstOrDefaultAsync(dto.UserId) == null)
            {
                throw new EntryNotFoundException(nameof(Database.Entities.User), dto.UserId);
            }

            if (await _leaveRequestTypeRepository.GetFirstOrDefaultAsync(dto.LeaveRequestTypeId) == null)
            {
                throw new EntryNotFoundException(nameof(Database.Entities.LeaveRequestType), dto.LeaveRequestTypeId);
            }

            var leaveRequest = new LeaveRequest
            {
                UserId = dto.UserId,
                LeaveRequestTypeId = dto.LeaveRequestTypeId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Reason = dto.Reason,
                RequestStatus = RequestStatus.Pending,
            };

            await _leaveRequestRepository.AddAsync(leaveRequest);
            return leaveRequest.ToCreateLeaveRequestResponseDto();
        }

        public async Task<List<LeaveRequestResponseDto>> GetRequestsByUserAsync(int userId)
        {
            var requests = await _leaveRequestRepository.GetAllAsync();
            var filtered = requests.Where(r => r.UserId == userId)
                                   .Select(r => r.ToLeaveRequestResponseDto())
                                   .ToList();
            return filtered;
        }

        public async Task<List<LeaveRequestResponseDto>> GetAllRequestsAsync()
        {
            var requests = await _leaveRequestRepository.GetAllAsync();
            return requests.Select(r => r.ToLeaveRequestResponseDto()).ToList();
        }

        public async Task<LeaveRequestResponseDto> GetRequestByIdAsync(int id)
        {
            var request = await _leaveRequestRepository.GetFirstOrDefaultAsync(id);

            if (request == null)
                throw new EntryNotFoundException(nameof(LeaveRequest), id);

            return request.ToLeaveRequestResponseDto();
        }

        public async Task ReviewLeaveRequestAsync(int id, ReviewLeaveRequestDto dto, int managerId)
        {
            var request = await _leaveRequestRepository.GetFirstOrDefaultAsync(id);

            if (request == null)
                throw new EntryNotFoundException(nameof(LeaveRequest), id);

            request.RequestStatus = dto.RequestStatus;
            request.ReviewerComment = dto.ReviewerComment;
            request.ReviewerId = managerId;


            request.ModifiedAt = DateTime.UtcNow;

            await _leaveRequestRepository.UpdateAsync(request);
        }

        public async Task<bool> DeleteLeaveRequestAsync(int id)
        {
            if(await _leaveRequestRepository.GetFirstOrDefaultAsync(id) == null)
            {
                throw new EntryNotFoundException(nameof(LeaveRequest), id);
            }

            await _leaveRequestRepository.DeleteAsync(id);
            return true;
        }

        public async Task<LeaveRequestResponseDto> UpdateLeaveRequestAsync(int id, UpdateLeaveRequestDto dto)
        {
            LeaveRequest? existing = await _leaveRequestRepository.GetFirstOrDefaultAsync(id);

            if(existing == null)
            {
                throw new EntryNotFoundException(nameof(LeaveRequest), id);
            }

            if(dto.UserId != null && await _userRepository.GetFirstOrDefaultAsync((int)dto.UserId) == null)
            {
                throw new EntryNotFoundException(nameof(Database.Entities.User), dto.UserId);
            }

            if(dto.ReviewerId != null && await _userRepository.GetFirstOrDefaultAsync((int)dto.ReviewerId) == null)
            {
                throw new EntryNotFoundException(nameof(Database.Entities.User), dto.ReviewerId);
            }

            if(dto.LeaveRequestTypeId != null && await _leaveRequestTypeRepository.GetFirstOrDefaultAsync((int)dto.LeaveRequestTypeId) == null)
            {
                throw new EntryNotFoundException(nameof(Database.Entities.LeaveRequestType), dto.LeaveRequestTypeId);
            }

            PatchHelper.PatchRequestToEntity.PatchFrom<UpdateLeaveRequestDto, LeaveRequest>(existing, dto);
            existing.ModifiedAt = DateTime.UtcNow;

            await _leaveRequestRepository.SaveChangesAsync();
            return existing.ToLeaveRequestResponseDto();
        }

        
        public async Task<List<LeaveRequestResponseDto>> GetLeaveRequestsForManagerAsync(int managerId)
        {
            var employees = await _userRepository.GetUsersByManagerIdAsync(managerId);
            var employeeIds = employees.Select(e => e.Id).ToList();

            var allRequests = await _leaveRequestRepository.GetAllAsync();
            var filtered = allRequests
                .Where(r => employeeIds.Contains(r.UserId))
                .Select(r => r.ToLeaveRequestResponseDto())
                .ToList();

            return filtered;
        }

        public async Task<PagedResponseDto<LeaveRequestResponseDto>> GetAllLeaveRequestsFilteredAsync(int managerId, QueriedLeaveRequestRequestDto payload)
        {
            var employees = await _userRepository.GetUsersByManagerIdAsync(managerId);
            var employeeIds = employees.Select(e => e.Id).ToList();

            var (result, totalCount) = await _leaveRequestRepository.GetAllLeaveRequestsWithRelationshipsFilteredAsync(employeeIds, payload.LastName, payload.Email, payload.PagedQueryParams.ToQueryParams());

            if (result == null || !result.Any())
                return new PagedResponseDto<LeaveRequestResponseDto>
                {
                    Data = new List<LeaveRequestResponseDto>(),
                    Page = payload.PagedQueryParams.Page ?? 1,
                    PageSize = payload.PagedQueryParams.PageSize ?? 1,
                    TotalPages = 0
                };

            return new PagedResponseDto<LeaveRequestResponseDto>
            {
                Data = result.Select(lr => new LeaveRequestResponseDto
                {
                    Id = lr.Id,
                    UserId = lr.UserId,
                    FullName = lr.User?.FullName ?? string.Empty,
                    ReviewerId = lr.ReviewerId,
                    LeaveRequestTypeId = lr.LeaveRequestTypeId,
                    StartDate = lr.StartDate,
                    EndDate = lr.EndDate,
                    Reason = lr.Reason ?? string.Empty,
                    RequestStatus = lr.RequestStatus,
                    ReviewerComment = lr.ReviewerComment ?? string.Empty,
                }),
                Page = payload.PagedQueryParams.Page ?? 1,
                PageSize = payload.PagedQueryParams.PageSize ?? 1,
                TotalPages = payload.PagedQueryParams.PageSize != null ?
                    (int)Math.Ceiling((double)totalCount / (int)payload.PagedQueryParams.PageSize) : 1
            };
        }
    }
}
