using ManagementSimulator.Core.Dtos.Requests.SecondaryManagers;
using ManagementSimulator.Core.Dtos.Responses.SecondaryManager;
using ManagementSimulator.Core.Dtos.Responses.User;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Services.Interfaces
{
    public interface ISecondaryManagerService
    {
        Task<SecondaryManagerResponseDto> AssignSecondaryManagerAsync(CreateSecondaryManagerRequest request, int assignedByAdminId);
        Task<SecondaryManagerResponseDto> UpdateSecondaryManagerAsync(int employeeId, int secondaryManagerId, DateTime startDate, UpdateSecondaryManagerRequest request);
        Task RemoveSecondaryManagerAsync(int employeeId, int secondaryManagerId, DateTime startDate);
        Task<List<SecondaryManagerResponseDto>> GetAllSecondaryManagersAsync();
        Task<List<SecondaryManagerResponseDto>> GetSecondaryManagersForEmployeeAsync(int employeeId);
        Task<List<SecondaryManagerResponseDto>> GetActiveSecondaryManagersForEmployeeAsync(int employeeId);
        Task<List<UserResponseDto>> GetEmployeesWithActiveSecondaryManagerAsync(int secondaryManagerId);
        Task<List<SecondaryManagerResponseDto>> GetSecondaryManagersAssignedByAdminAsync(int adminId);
        Task<SecondaryManagerResponseDto?> GetSecondaryManagerByIdAsync(int employeeId, int secondaryManagerId, DateTime startDate);
        Task<List<SecondaryManagerResponseDto>> GetExpiredSecondaryManagersAsync();
        Task<bool> HasActiveSecondaryManagerAsync(int employeeId, int secondaryManagerId);
        Task<bool> ValidateSecondaryManagerAssignmentAsync(int employeeId, int secondaryManagerId, DateTime startDate, DateTime endDate);
    }
}