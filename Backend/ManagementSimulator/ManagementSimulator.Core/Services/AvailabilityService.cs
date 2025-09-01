using ManagementSimulator.Core.Services.Interfaces;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Enums;
using ManagementSimulator.Database.Repositories.Intefaces;
using System.Linq;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Services
{
    public class AvailabilityService : IAvailabilityService
    {
        private readonly IUserRepository _userRepository;
        private readonly IProjectRepository _projectRepository;

        public AvailabilityService(IUserRepository userRepository, IProjectRepository projectRepository)
        {
            _userRepository = userRepository;
            _projectRepository = projectRepository;
        }

        public float CalculateTotalAvailability(EmploymentType employmentType)
        {
            return employmentType switch
            {
                EmploymentType.FullTime => 1.0f, // 100% availability (1.0 FTE)
                EmploymentType.PartTime => 0.5f, // 50% availability (0.5 FTE)
                _ => 1.0f // Default to full-time if unknown
            };
        }

        public async Task<float> CalculateRemainingAvailabilityAsync(int userId)
        {
            var user = await _userRepository.GetFirstOrDefaultAsync(userId);
            if (user == null)
                return 0f;

            var totalAvailability = CalculateTotalAvailability(user.EmploymentType);

            // Get all project assignments for the user
            var userProjects = await _projectRepository.GetUserProjectsByUserIdAsync(userId);

            // Calculate total allocated capacity in FTE terms
            var totalAllocatedFTE = 0f;
            foreach (var userProject in userProjects)
            {
                // Convert percentage to actual FTE allocation
                // For both full-time and part-time, percentage is relative to their total availability
                var actualFTEAllocation = (userProject.TimePercentagePerProject / 100f) * totalAvailability;
                totalAllocatedFTE += actualFTEAllocation;
            }

            // Calculate remaining availability
            var remainingAvailability = totalAvailability - totalAllocatedFTE;

            // Ensure we don't return negative availability
            return remainingAvailability > 0 ? remainingAvailability : 0f;
        }

        public float CalculateEffectiveAllocation(float projectAllocationPercentage, EmploymentType employmentType)
        {
            // This method converts a project allocation percentage to actual FTE usage
            // For both employment types, the percentage is relative to their total capacity
            var totalAvailability = CalculateTotalAvailability(employmentType);
            return (projectAllocationPercentage / 100f) * totalAvailability;
        }

        public async Task<bool> UpdateUserAvailabilityAsync(int userId)
        {
            var user = await _userRepository.GetFirstOrDefaultAsync(userId);
            if (user == null)
                return false;

            // Calculate and update availability values
            user.TotalAvailability = CalculateTotalAvailability(user.EmploymentType);
            user.RemainingAvailability = await CalculateRemainingAvailabilityAsync(userId);

            await _userRepository.SaveChangesAsync();
            return true;
        }

        public async Task<int> UpdateAllUsersAvailabilityAsync()
        {
            var users = await _userRepository.GetAllAsync();
            var updateCount = 0;

            foreach (var user in users)
            {
                var previousTotal = user.TotalAvailability;
                var previousRemaining = user.RemainingAvailability;

                user.TotalAvailability = CalculateTotalAvailability(user.EmploymentType);
                user.RemainingAvailability = await CalculateRemainingAvailabilityAsync(user.Id);

                if (previousTotal != user.TotalAvailability || previousRemaining != user.RemainingAvailability)
                {
                    updateCount++;
                }
            }

            if (updateCount > 0)
            {
                await _userRepository.SaveChangesAsync();
            }

            return updateCount;
        }

        public async Task<bool> ValidateProjectAssignmentAsync(int userId, float projectAllocationPercentage, int? excludeProjectId = null)
        {
            var user = await _userRepository.GetFirstOrDefaultAsync(userId);
            if (user == null)
                return false;

            var totalAvailability = CalculateTotalAvailability(user.EmploymentType);

            // Get all project assignments for the user, excluding the specified project if provided
            var userProjects = await _projectRepository.GetUserProjectsByUserIdAsync(userId);
            if (excludeProjectId.HasValue)
            {
                userProjects = userProjects.Where(up => up.ProjectId != excludeProjectId.Value).ToList();
            }

            // Calculate current total allocated FTE
            var currentTotalAllocatedFTE = 0f;
            foreach (var userProject in userProjects)
            {
                var actualFTEAllocation = (userProject.TimePercentagePerProject / 100f) * totalAvailability;
                currentTotalAllocatedFTE += actualFTEAllocation;
            }

            // Calculate the FTE allocation for the new assignment
            var newFTEAllocation = (projectAllocationPercentage / 100f) * totalAvailability;

            // Check if the total allocation would exceed their total availability
            var totalAllocatedFTE = currentTotalAllocatedFTE + newFTEAllocation;

            return totalAllocatedFTE <= totalAvailability;
        }
    }
}