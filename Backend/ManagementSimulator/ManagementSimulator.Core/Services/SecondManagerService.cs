using ManagementSimulator.Core.Dtos.Requests.SecondManager;
using ManagementSimulator.Core.Dtos.Responses;
using ManagementSimulator.Core.Dtos.Responses.User;
using ManagementSimulator.Core.Services.Interfaces;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Repositories.Intefaces;
using ManagementSimulator.Database.Repositories.Interfaces;
using ManagementSimulator.Infrastructure.Exceptions;

namespace ManagementSimulator.Core.Services
{
    public class SecondManagerService : ISecondManagerService
    {
        private readonly ISecondManagerRepository _secondManagerRepository;
        private readonly IUserRepository _userRepository;

        public SecondManagerService(ISecondManagerRepository secondManagerRepository, IUserRepository userRepository)
        {
            _secondManagerRepository = secondManagerRepository;
            _userRepository = userRepository;
        }

        public async Task<List<SecondManagerResponseDto>> GetAllSecondManagersAsync()
        {
            var secondManagers = await _secondManagerRepository.GetAllSecondManagersAsync();
            return secondManagers.Select(MapToResponseDto).ToList();
        }

        public async Task<List<SecondManagerResponseDto>> GetActiveSecondManagersAsync()
        {
            var activeSecondManagers = await _secondManagerRepository.GetActiveSecondManagersAsync();
            return activeSecondManagers.Select(MapToResponseDto).ToList();
        }

        public async Task<List<SecondManagerResponseDto>> GetSecondManagersByReplacedManagerIdAsync(int replacedManagerId)
        {
            var secondManagers = await _secondManagerRepository.GetSecondManagersByReplacedManagerIdAsync(replacedManagerId);
            return secondManagers.Select(MapToResponseDto).ToList();
        }

        public async Task<SecondManagerResponseDto> CreateSecondManagerAsync(CreateSecondManagerRequestDto request)
        {
            // Validez că utilizatorii există
            var secondManagerEmployee = await _userRepository.GetUserByIdAsync(request.SecondManagerEmployeeId);
            if (secondManagerEmployee == null)
                throw new EntryNotFoundException(nameof(User), request.SecondManagerEmployeeId);

            var replacedManager = await _userRepository.GetUserByIdAsync(request.ReplacedManagerId);
            if (replacedManager == null)
                throw new EntryNotFoundException(nameof(User), request.ReplacedManagerId);

            // Validez că data de sfârșit este după data de început
            if (request.EndDate <= request.StartDate)
                throw new InvalidDateRangeException("Data de sfârșit trebuie să fie după data de început");

            var secondManager = new SecondManager
            {
                SecondManagerEmployeeId = request.SecondManagerEmployeeId,
                ReplacedManagerId = request.ReplacedManagerId,
                StartDate = request.StartDate,
                EndDate = request.EndDate
            };

            await _secondManagerRepository.AddSecondManagerAsync(secondManager);

            // Reiau entitatea cu relațiile încărcate
            var createdSecondManager = await _secondManagerRepository.GetSecondManagerAsync(
                request.SecondManagerEmployeeId,
                request.ReplacedManagerId,
                request.StartDate);

            return MapToResponseDto(createdSecondManager!);
        }

        public async Task<SecondManagerResponseDto> UpdateSecondManagerAsync(int secondManagerEmployeeId, int replacedManagerId, DateTime startDate, UpdateSecondManagerRequestDto request)
        {
            var secondManager = await _secondManagerRepository.GetSecondManagerAsync(secondManagerEmployeeId, replacedManagerId, startDate);
            if (secondManager == null)
                throw new EntryNotFoundException(nameof(SecondManager), $"{secondManagerEmployeeId}-{replacedManagerId}-{startDate}");

            // Validez că noua dată de sfârșit este după data de început
            if (request.NewEndDate <= secondManager.StartDate)
                throw new InvalidDateRangeException("Noua dată de sfârșit trebuie să fie după data de început");

            secondManager.EndDate = request.NewEndDate;
            await _secondManagerRepository.UpdateSecondManagerAsync(secondManager);

            return MapToResponseDto(secondManager);
        }

        public async Task DeleteSecondManagerAsync(int secondManagerEmployeeId, int replacedManagerId, DateTime startDate)
        {
            var secondManager = await _secondManagerRepository.GetSecondManagerAsync(secondManagerEmployeeId, replacedManagerId, startDate);
            if (secondManager == null)
                throw new EntryNotFoundException(nameof(SecondManager), $"{secondManagerEmployeeId}-{replacedManagerId}-{startDate}");

            await _secondManagerRepository.DeleteSecondManagerAsync(secondManagerEmployeeId, replacedManagerId, startDate);
        }

        public async Task<bool> IsUserActingAsSecondManagerAsync(int userId)
        {
            var activeSecondManagers = await _secondManagerRepository.GetActiveSecondManagersAsync();
            return activeSecondManagers.Any(sm => sm.SecondManagerEmployeeId == userId);
        }

        public async Task<List<int>> GetManagersBeingReplacedByUserAsync(int userId)
        {
            var activeSecondManagers = await _secondManagerRepository.GetActiveSecondManagersAsync();
            return activeSecondManagers
                .Where(sm => sm.SecondManagerEmployeeId == userId)
                .Select(sm => sm.ReplacedManagerId)
                .ToList();
        }

        private SecondManagerResponseDto MapToResponseDto(SecondManager secondManager)
        {
            return new SecondManagerResponseDto
            {
                SecondManagerEmployeeId = secondManager.SecondManagerEmployeeId,
                SecondManagerEmployee = new UserResponseDto
                {
                    Id = secondManager.SecondManagerEmployee.Id,
                    FirstName = secondManager.SecondManagerEmployee.FirstName,
                    LastName = secondManager.SecondManagerEmployee.LastName,
                    Email = secondManager.SecondManagerEmployee.Email
                },
                ReplacedManagerId = secondManager.ReplacedManagerId,
                ReplacedManager = new UserResponseDto
                {
                    Id = secondManager.ReplacedManager.Id,
                    FirstName = secondManager.ReplacedManager.FirstName,
                    LastName = secondManager.ReplacedManager.LastName,
                    Email = secondManager.ReplacedManager.Email
                },
                StartDate = secondManager.StartDate,
                EndDate = secondManager.EndDate,
                IsActive = secondManager.IsActive
            };
        }
    }
}