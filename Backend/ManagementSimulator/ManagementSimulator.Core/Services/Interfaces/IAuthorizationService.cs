namespace ManagementSimulator.Core.Services.Interfaces
{
    public interface IResourceAuthorizationService
    {
        Task<bool> CanManagerAccessUserDataAsync(int managerId, int userId);
        Task<bool> CanManagerAccessLeaveRequestAsync(int managerId, int leaveRequestId);
        Task<bool> CanUserAccessOwnLeaveRequestAsync(int userId, int leaveRequestId);
    }
}