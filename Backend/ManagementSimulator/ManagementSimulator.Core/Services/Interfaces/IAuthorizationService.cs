namespace ManagementSimulator.Core.Services.Interfaces
{
    public interface IResourceAuthorizationService
    {
        Task<bool> CanManagerAccessUserDataAsync(int managerId, int userId);
        Task<bool> CanManagerAccessLeaveRequestAsync(int managerId, int leaveRequestId);
        Task<bool> CanUserAccessOwnLeaveRequestAsync(int userId, int leaveRequestId);
        Task<bool> CanManagerModifyDataAsync(int managerId);
        Task<bool> IsManagerTemporarilyReplacedAsync(int managerId);
        Task<int?> GetActiveSecondManagerForManagerAsync(int managerId);
        Task<bool> IsUserActingAsSecondManagerAsync(int userId);
    }
}