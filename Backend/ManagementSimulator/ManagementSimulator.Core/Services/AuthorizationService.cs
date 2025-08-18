using ManagementSimulator.Core.Services.Interfaces;
using ManagementSimulator.Database.Repositories.Intefaces;

namespace ManagementSimulator.Core.Services
{
    public class ResourceAuthorizationService : IResourceAuthorizationService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILeaveRequestRepository _leaveRequestRepository;

        public ResourceAuthorizationService(IUserRepository userRepository, ILeaveRequestRepository leaveRequestRepository)
        {
            _userRepository = userRepository;
            _leaveRequestRepository = leaveRequestRepository;
        }

        public async Task<bool> CanManagerAccessUserDataAsync(int managerId, int userId)
        {
            // Check if the user is a direct subordinate of the manager
            var subordinates = await _userRepository.GetUsersByManagerIdAsync(managerId);
            return subordinates.Any(u => u.Id == userId);
        }

        public async Task<bool> CanManagerAccessLeaveRequestAsync(int managerId, int leaveRequestId)
        {
            // Get the leave request to find the user who created it
            var leaveRequest = await _leaveRequestRepository.GetFirstOrDefaultAsync(leaveRequestId);
            if (leaveRequest == null)
            {
                return false;
            }

            // Check if the manager can access this user's data
            return await CanManagerAccessUserDataAsync(managerId, leaveRequest.UserId);
        }

        public async Task<bool> CanUserAccessOwnLeaveRequestAsync(int userId, int leaveRequestId)
        {
            // Check if the leave request belongs to the user
            var leaveRequest = await _leaveRequestRepository.GetFirstOrDefaultAsync(leaveRequestId);
            return leaveRequest?.UserId == userId;
        }
    }
}