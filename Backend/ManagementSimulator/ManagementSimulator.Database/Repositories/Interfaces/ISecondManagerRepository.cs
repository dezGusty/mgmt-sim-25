using ManagementSimulator.Database.Entities;

namespace ManagementSimulator.Database.Repositories.Interfaces
{
    public interface ISecondManagerRepository
    {
        Task<List<SecondManager>> GetAllSecondManagersAsync();
        Task<List<SecondManager>> GetActiveSecondManagersAsync();
        Task<List<SecondManager>> GetSecondManagersByReplacedManagerIdAsync(int replacedManagerId);
        Task<SecondManager?> GetSecondManagerAsync(int secondManagerEmployeeId, int replacedManagerId, DateTime startDate);
        Task AddSecondManagerAsync(SecondManager secondManager);
        Task UpdateSecondManagerAsync(SecondManager secondManager);
        Task DeleteSecondManagerAsync(int secondManagerEmployeeId, int replacedManagerId, DateTime startDate);
        Task<List<User>> GetEmployeesForActiveSecondManagerAsync(int secondManagerEmployeeId);
        Task SaveChangesAsync();
    }
} 