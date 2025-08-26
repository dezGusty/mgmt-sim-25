using ManagementSimulator.Core.Services.Interfaces;
using ManagementSimulator.Database.Repositories.Intefaces;
using ManagementSimulator.Database.Repositories.Interfaces;

namespace ManagementSimulator.Core.Services
{
    public class ResourceAuthorizationService : IResourceAuthorizationService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILeaveRequestRepository _leaveRequestRepository;
        private readonly ISecondManagerRepository _secondManagerRepository;

        public ResourceAuthorizationService(IUserRepository userRepository, ILeaveRequestRepository leaveRequestRepository, ISecondManagerRepository secondManagerRepository)
        {
            _userRepository = userRepository;
            _leaveRequestRepository = leaveRequestRepository;
            _secondManagerRepository = secondManagerRepository;
        }

        public async Task<bool> CanManagerAccessUserDataAsync(int managerId, int userId)
        {
            var subordinates = await _userRepository.GetUsersByManagerIdAsync(managerId);
            return subordinates.Any(u => u.Id == userId);
        }

        public async Task<bool> CanManagerAccessLeaveRequestAsync(int managerId, int leaveRequestId)
        {
            var leaveRequest = await _leaveRequestRepository.GetFirstOrDefaultAsync(leaveRequestId);
            if (leaveRequest == null)
            {
                return false;
            }

            return await CanManagerAccessUserDataAsync(managerId, leaveRequest.UserId);
        }

        public async Task<bool> CanUserAccessOwnLeaveRequestAsync(int userId, int leaveRequestId)
        {
            var leaveRequest = await _leaveRequestRepository.GetFirstOrDefaultAsync(leaveRequestId);
            return leaveRequest?.UserId == userId;
        }

        public async Task<bool> CanManagerModifyDataAsync(int managerId)
        {
            return !await IsManagerTemporarilyReplacedAsync(managerId);
        }

        public async Task<bool> IsManagerTemporarilyReplacedAsync(int managerId)
        {
            var activeSecondManagers = await _secondManagerRepository.GetActiveSecondManagersAsync();
            return activeSecondManagers.Any(sm => sm.ReplacedManagerId == managerId);
        }

        public async Task<int?> GetActiveSecondManagerForManagerAsync(int managerId)
        {
            var activeSecondManagers = await _secondManagerRepository.GetActiveSecondManagersAsync();
            var activeSecondManager = activeSecondManagers.FirstOrDefault(sm => sm.ReplacedManagerId == managerId);
            return activeSecondManager?.SecondManagerEmployeeId;
        }

        public async Task<bool> IsUserActingAsSecondManagerAsync(int userId)
        {
            var activeSecondManagers = await _secondManagerRepository.GetActiveSecondManagersAsync();
            return activeSecondManagers.Any(sm => sm.SecondManagerEmployeeId == userId);
        }
    }
}