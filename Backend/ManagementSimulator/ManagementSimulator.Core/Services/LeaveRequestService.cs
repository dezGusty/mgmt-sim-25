using ManagementSimulator.Core.Dtos.Requests.LeaveRequest;
using ManagementSimulator.Core.Dtos.Requests.LeaveRequests;
using ManagementSimulator.Core.Dtos.Responses;
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

            var leaveRequestType = await _leaveRequestTypeRepository.GetFirstOrDefaultAsync(dto.LeaveRequestTypeId);
            if (leaveRequestType == null)
            {
                throw new EntryNotFoundException(nameof(Database.Entities.LeaveRequestType), dto.LeaveRequestTypeId);
            }

            if (dto.EndDate < dto.StartDate)
            {
                throw new InvalidDateRangeException("End date cannot be before start date.");
            }

            var overlappingRequests = await _leaveRequestRepository.GetOverlappingRequestsAsync(dto.UserId, dto.StartDate, dto.EndDate);
            var hasConflictingRequest = overlappingRequests.Any(r => r.RequestStatus == RequestStatus.Pending || r.RequestStatus == RequestStatus.Approved);
            
            if (hasConflictingRequest)
            {
                throw new LeaveRequestOverlapException("Employee already has a pending or approved leave request for this period.");
            }

            if (leaveRequestType.MaxDays.HasValue)
            {
                var requestedDays = CalculateLeaveDays(dto.StartDate, dto.EndDate);
                var currentYear = DateTime.Now.Year;
                var remainingDaysInfo = await GetRemainingLeaveDaysAsync(dto.UserId, dto.LeaveRequestTypeId, currentYear);
                
                if (remainingDaysInfo.RemainingDays.HasValue && requestedDays > remainingDaysInfo.RemainingDays.Value)
                {
                    throw new InsufficientLeaveDaysException(
                        dto.UserId, 
                        dto.LeaveRequestTypeId, 
                        requestedDays, 
                        remainingDaysInfo.RemainingDays.Value,
                        $"Insufficient leave days. Requested: {requestedDays}, Available: {remainingDaysInfo.RemainingDays.Value}");
                }
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
            
            var savedLeaveRequest = await _leaveRequestRepository.GetLeaveRequestWithDetailsAsync(leaveRequest.Id);
            return savedLeaveRequest.ToCreateLeaveRequestResponseDto();
        }

        public async Task<CreateLeaveRequestResponseDto> AddLeaveRequestByEmployeeAsync(CreateLeaveRequestByEmployeeDto dto, int userId)
        {
            if (await _userRepository.GetFirstOrDefaultAsync(userId) == null)
            {
                throw new EntryNotFoundException(nameof(Database.Entities.User), userId);
            }

            var leaveRequestType = await _leaveRequestTypeRepository.GetFirstOrDefaultAsync(dto.LeaveRequestTypeId);
            if (leaveRequestType == null)
            {
                throw new EntryNotFoundException(nameof(Database.Entities.LeaveRequestType), dto.LeaveRequestTypeId);
            }

            if (dto.EndDate < dto.StartDate)
            {
                throw new InvalidDateRangeException("End date cannot be before start date.");
            }

            var overlappingRequests = await _leaveRequestRepository.GetOverlappingRequestsAsync(userId, dto.StartDate, dto.EndDate);
            var hasConflictingRequest = overlappingRequests.Any(r => r.RequestStatus == RequestStatus.Pending || r.RequestStatus == RequestStatus.Approved);
            
            if (hasConflictingRequest)
            {
                throw new LeaveRequestOverlapException("You already have a pending or approved leave request for this period.");
            }

            if (leaveRequestType.MaxDays.HasValue)
            {
                var requestedDays = CalculateLeaveDays(dto.StartDate, dto.EndDate);
                var currentYear = DateTime.Now.Year;
                var remainingDaysInfo = await GetRemainingLeaveDaysAsync(userId, dto.LeaveRequestTypeId, currentYear);
                
                if (remainingDaysInfo.RemainingDays.HasValue && requestedDays > remainingDaysInfo.RemainingDays.Value)
                {
                    throw new InsufficientLeaveDaysException(
                        userId, 
                        dto.LeaveRequestTypeId, 
                        requestedDays, 
                        remainingDaysInfo.RemainingDays.Value,
                        $"Insufficient leave days. Requested: {requestedDays}, Available: {remainingDaysInfo.RemainingDays.Value}");
                }
            }

            var leaveRequest = new LeaveRequest
            {
                UserId = userId,
                LeaveRequestTypeId = dto.LeaveRequestTypeId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Reason = dto.Reason,
                RequestStatus = RequestStatus.Pending,
            };

            await _leaveRequestRepository.AddAsync(leaveRequest);

            var savedLeaveRequest = await _leaveRequestRepository.GetLeaveRequestWithDetailsAsync(leaveRequest.Id);
            return savedLeaveRequest.ToCreateLeaveRequestResponseDto();
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

            var allRequests = await _leaveRequestRepository.GetAllWithRelationshipsAsync();
            var filtered = allRequests
                .Where(r => employeeIds.Contains(r.UserId))
                .Select(r => r.ToLeaveRequestResponseDto())
                .ToList();

            return filtered;
        }
        //         public async Task<List<LeaveRequestResponseDto>> GetLeaveRequestsForManagerAsync(int managerId)
        // {
        //     var employees = await _userRepository.GetUsersByManagerIdAsync(managerId);
        //     var employeeIds = employees.Select(e => e.Id).ToList();

        //     var allRequests = await _leaveRequestRepository.GetAllWithRelationshipsAsync();
        //     var today = DateTime.UtcNow.Date;
            
        //     var filtered = allRequests
        //         .Where(r => employeeIds.Contains(r.UserId))
        //         .Where(r => !(r.RequestStatus == RequestStatus.Pending && r.StartDate < today))
        //         .Select(r => r.ToLeaveRequestResponseDto())
        //         .ToList();

        //     return filtered;
        // }

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

        public async Task CancelLeaveRequestAsync(int requestId, int userId)
        {
            var request = await _leaveRequestRepository.GetFirstOrDefaultAsync(requestId);

            if (request == null)
                throw new EntryNotFoundException(nameof(LeaveRequest), requestId);

            if (request.UserId != userId)
                throw new UnauthorizedAccessException("Cannot cancel other user's request");

            if (request.RequestStatus != RequestStatus.Pending)
                throw new InvalidOperationException("Can only cancel pending requests");

            request.RequestStatus = RequestStatus.Canceled;
            request.ModifiedAt = DateTime.UtcNow;

            await _leaveRequestRepository.UpdateAsync(request);
        }

        public async Task<RemainingLeaveDaysResponseDto> GetRemainingLeaveDaysAsync(int userId, int leaveRequestTypeId, int year)
        {
            var user = await _userRepository.GetFirstOrDefaultAsync(userId);
            if (user == null)
            {
                throw new EntryNotFoundException(nameof(Database.Entities.User), userId);
            }

            var leaveRequestType = await _leaveRequestTypeRepository.GetFirstOrDefaultAsync(leaveRequestTypeId);
            if (leaveRequestType == null)
            {
                throw new EntryNotFoundException(nameof(Database.Entities.LeaveRequestType), leaveRequestTypeId);
            }

            var leaveRequests = await _leaveRequestRepository.GetLeaveRequestsByUserAndTypeAsync(userId, leaveRequestTypeId, year);

            int daysUsed = 0;
            var usedLeaveRequests = new List<LeaveRequestSummaryDto>();

            foreach (var request in leaveRequests)
            {
                var requestDays = CalculateLeaveDays(request.StartDate, request.EndDate);
                daysUsed += requestDays;

                usedLeaveRequests.Add(new LeaveRequestSummaryDto
                {
                    Id = request.Id,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    DaysCount = requestDays,
                    Status = request.RequestStatus.ToString()
                });
            }

            int? remainingDays = null;
            bool hasUnlimitedDays = leaveRequestType.MaxDays == null;

            if (!hasUnlimitedDays)
            {
                remainingDays = leaveRequestType.MaxDays - daysUsed;
            }

            return new RemainingLeaveDaysResponseDto
            {
                UserId = userId,
                LeaveRequestTypeId = leaveRequestTypeId,
                LeaveRequestTypeName = leaveRequestType.Title ?? string.Empty,
                MaxDaysAllowed = leaveRequestType.MaxDays,
                DaysUsed = daysUsed,
                RemainingDays = remainingDays,
                HasUnlimitedDays = hasUnlimitedDays,
                UsedLeaveRequests = usedLeaveRequests
            };
        }

        public async Task<RemainingLeaveDaysResponseDto> GetRemainingLeaveDaysForPeriodAsync(int userId, int leaveRequestTypeId, DateTime startDate, DateTime endDate)
        {
            var user = await _userRepository.GetFirstOrDefaultAsync(userId);
            if (user == null)
            {
                throw new EntryNotFoundException(nameof(Database.Entities.User), userId);
            }

            var leaveRequestType = await _leaveRequestTypeRepository.GetFirstOrDefaultAsync(leaveRequestTypeId);
            if (leaveRequestType == null)
            {
                throw new EntryNotFoundException(nameof(Database.Entities.LeaveRequestType), leaveRequestTypeId);
            }

            var year = startDate.Year;

            var existingLeaveRequests = await _leaveRequestRepository.GetLeaveRequestsByUserAndTypeAsync(userId, leaveRequestTypeId, year);

            int daysUsed = 0;
            var usedLeaveRequests = new List<LeaveRequestSummaryDto>();

            foreach (var request in existingLeaveRequests)
            {
                var requestDays = CalculateLeaveDays(request.StartDate, request.EndDate);
                daysUsed += requestDays;

                usedLeaveRequests.Add(new LeaveRequestSummaryDto
                {
                    Id = request.Id,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    DaysCount = requestDays,
                    Status = request.RequestStatus.ToString()
                });
            }

            var requestedDays = CalculateLeaveDays(startDate, endDate);

            int? remainingDays = null;
            bool hasUnlimitedDays = leaveRequestType.MaxDays == null;

            if (!hasUnlimitedDays)
            {
                remainingDays = leaveRequestType.MaxDays - daysUsed - requestedDays;
            }

            return new RemainingLeaveDaysResponseDto
            {
                UserId = userId,
                LeaveRequestTypeId = leaveRequestTypeId,
                LeaveRequestTypeName = leaveRequestType.Title ?? string.Empty,
                MaxDaysAllowed = leaveRequestType.MaxDays,
                DaysUsed = daysUsed + requestedDays, 
                RemainingDays = remainingDays,
                HasUnlimitedDays = hasUnlimitedDays,
                UsedLeaveRequests = usedLeaveRequests
            };
        }

        private int CalculateLeaveDays(DateTime startDate, DateTime endDate)
        {
            int workingDays = 0;
            DateTime currentDate = startDate;

            while (currentDate <= endDate)
            {
                if (currentDate.DayOfWeek != DayOfWeek.Saturday && currentDate.DayOfWeek != DayOfWeek.Sunday)
                {
                    workingDays++;
                }
                currentDate = currentDate.AddDays(1);
            }

            return workingDays;
        }
    }
}
