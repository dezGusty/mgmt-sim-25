using ManagementSimulator.Core.Dtos.Requests.LeaveRequest;
using ManagementSimulator.Core.Dtos.Requests.LeaveRequests;
using ManagementSimulator.Core.Dtos.Responses;
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

        public async Task<LeaveRequestResponseDto> AddLeaveRequestAsync(CreateLeaveRequestRequestDto dto)
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
                IsApproved = null
            };

            await _leaveRequestRepository.AddAsync(leaveRequest);
            return leaveRequest.ToLeaveRequestResponseDto();
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

        public async Task ReviewLeaveRequestAsync(int id, ReviewLeaveRequestDto dto)
        {
            var request = await _leaveRequestRepository.GetFirstOrDefaultAsync(id);

            if (request == null)
                throw new EntryNotFoundException(nameof(LeaveRequest), dto.Id);

            request.IsApproved = dto.IsApproved;
            request.RequestStatus = dto.RequestStatus;
            request.ReviewerComment = dto.ReviewerComment;

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
    }
}
