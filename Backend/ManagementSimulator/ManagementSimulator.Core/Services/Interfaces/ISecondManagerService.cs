using ManagementSimulator.Core.Dtos.Requests.SecondManager;
using ManagementSimulator.Core.Dtos.Responses;

namespace ManagementSimulator.Core.Services.Interfaces
{
    public interface ISecondManagerService
    {
        Task<List<SecondManagerResponseDto>> GetAllSecondManagersAsync();
        Task<List<SecondManagerResponseDto>> GetActiveSecondManagersAsync();
        Task<List<SecondManagerResponseDto>> GetSecondManagersByReplacedManagerIdAsync(int replacedManagerId);
        Task<SecondManagerResponseDto> CreateSecondManagerAsync(CreateSecondManagerRequestDto request);
        Task<SecondManagerResponseDto> UpdateSecondManagerAsync(int secondManagerEmployeeId, int replacedManagerId, DateTime startDate, UpdateSecondManagerRequestDto request);
        Task DeleteSecondManagerAsync(int secondManagerEmployeeId, int replacedManagerId, DateTime startDate);
        Task<bool> IsUserActingAsSecondManagerAsync(int userId);
        Task<List<int>> GetManagersBeingReplacedByUserAsync(int userId);
    }
} 