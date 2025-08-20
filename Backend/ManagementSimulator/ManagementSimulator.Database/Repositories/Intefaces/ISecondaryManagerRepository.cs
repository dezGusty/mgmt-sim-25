using ManagementSimulator.Database.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ManagementSimulator.Database.Repositories.Intefaces
{
    public interface ISecondaryManagerRepository
    {
        Task<List<SecondaryManager>> GetAllSecondaryManagersAsync(bool includeDeleted = false, bool tracking = false);
        Task<List<SecondaryManager>> GetActiveSecondaryManagersForEmployeeAsync(int employeeId, bool tracking = false);
        Task<List<SecondaryManager>> GetSecondaryManagersForEmployeeAsync(int employeeId, bool includeDeleted = false, bool tracking = false);
        Task<List<User>> GetEmployeesWithActiveSecondaryManagerAsync(int secondaryManagerId, bool tracking = false);
        Task<SecondaryManager?> GetSecondaryManagerByIdAsync(int employeeId, int secondaryManagerId, DateTime startDate, bool includeDeleted = false, bool tracking = false);

        Task AddSecondaryManagerAsync(SecondaryManager secondaryManager);
        Task UpdateSecondaryManagerAsync(SecondaryManager secondaryManager);
        Task DeleteSecondaryManagerAsync(int employeeId, int secondaryManagerId, DateTime startDate);
        Task<List<SecondaryManager>> GetExpiredSecondaryManagersAsync(bool tracking = false);
        Task<bool> HasActiveSecondaryManagerAsync(int employeeId, int secondaryManagerId);
        Task<bool> HasOverlappingSecondaryManagerAsync(int employeeId, int secondaryManagerId, DateTime startDate, DateTime endDate, int? excludeAssignmentId = null);
        Task<bool> HasActiveSecondaryManagerForEmployeeAsync(int employeeId);
        Task<bool> HasActiveSecondaryManagerAssignmentAsync(int userId);
        Task SaveChangesAsync();
    }
}