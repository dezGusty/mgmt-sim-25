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
            
            // Calculate total allocated percentage based on employment type
            var totalAllocatedPercentage = 0f;
            foreach (var userProject in userProjects)
            {
                // For part-time employees, project allocations are more impactful
                // E.g., 50% project allocation for part-time = 100% of their capacity
                var effectiveAllocation = CalculateEffectiveAllocation(userProject.TimePercentagePerProject, user.EmploymentType);
                totalAllocatedPercentage += effectiveAllocation;
            }

            // Convert percentage to decimal and subtract from total availability
            var allocatedCapacity = totalAllocatedPercentage / 100f;
            var remainingAvailability = totalAvailability - allocatedCapacity;
            
            // Ensure we don't return negative availability
            return remainingAvailability > 0 ? remainingAvailability : 0f;
        }

        public float CalculateEffectiveAllocation(float projectAllocationPercentage, EmploymentType employmentType)
        {
            // For full-time employees, project allocation percentage is as-is
            // For part-time employees, project allocation has double the impact on their total capacity
            // Example: 50% project allocation for part-time employee = 100% of their 0.5 FTE capacity
            return employmentType switch
            {
                EmploymentType.FullTime => projectAllocationPercentage, // 1:1 ratio
                EmploymentType.PartTime => projectAllocationPercentage * 2f, // 2:1 ratio (double impact)
                _ => projectAllocationPercentage // Default behavior
            };
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

                // Only count as updated if values actually changed
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

            // Calculate current total allocated percentage
            var currentTotalAllocated = 0f;
            foreach (var userProject in userProjects)
            {
                var effectiveAllocation = CalculateEffectiveAllocation(userProject.TimePercentagePerProject, user.EmploymentType);
                currentTotalAllocated += effectiveAllocation;
            }

            // Calculate the effective allocation for the new assignment
            var newEffectiveAllocation = CalculateEffectiveAllocation(projectAllocationPercentage, user.EmploymentType);
            
            // Check if the total allocation would exceed 100% of their capacity
            var totalAllocatedPercentage = currentTotalAllocated + newEffectiveAllocation;
            var maxAllowedPercentage = totalAvailability * 100f; // Convert FTE to percentage

            return totalAllocatedPercentage <= maxAllowedPercentage;
        }
    }
}