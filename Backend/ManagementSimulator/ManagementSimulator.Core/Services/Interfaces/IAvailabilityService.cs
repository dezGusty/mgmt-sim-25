using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Enums;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Services.Interfaces
{
    public interface IAvailabilityService
    {
        /// <summary>
        /// Calculates the total availability for an employee based on their employment type
        /// Full-time: 1.0 FTE (100%), Part-time: 0.5 FTE (50%)
        /// </summary>
        /// <param name="employmentType">The employment type of the employee</param>
        /// <returns>Total availability as a float (0.5 for part-time, 1.0 for full-time)</returns>
        float CalculateTotalAvailability(EmploymentType employmentType);

        /// <summary>
        /// Calculates the remaining availability for a user after considering all project assignments
        /// The calculation is done in FTE terms where the percentage allocation is applied to their total availability
        /// Example: Part-time employee (0.5 FTE) with 50% project allocation uses 0.25 FTE, leaving 0.25 FTE remaining
        /// </summary>
        /// <param name="userId">The user ID to calculate remaining availability for</param>
        /// <returns>The remaining availability as a float</returns>
        Task<float> CalculateRemainingAvailabilityAsync(int userId);

        /// <summary>
        /// Updates the availability fields for a specific user
        /// </summary>
        /// <param name="userId">The user ID to update</param>
        /// <returns>True if updated successfully, false otherwise</returns>
        Task<bool> UpdateUserAvailabilityAsync(int userId);

        /// <summary>
        /// Updates the availability fields for all users
        /// </summary>
        /// <returns>The number of users updated</returns>
        Task<int> UpdateAllUsersAvailabilityAsync();

        /// <summary>
        /// Validates if a project assignment is possible without exceeding user availability
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="projectAllocationPercentage">The percentage to allocate to the project (0-100)</param>
        /// <param name="excludeProjectId">Optional project ID to exclude from calculation (for updates)</param>
        /// <returns>True if the assignment is valid, false if it would exceed availability</returns>
        Task<bool> ValidateProjectAssignmentAsync(int userId, float projectAllocationPercentage, int? excludeProjectId = null);

        /// <summary>
        /// Converts a project allocation percentage to actual FTE allocation based on employment type
        /// The percentage is relative to the user's total availability (employment type capacity)
        /// Example: 50% project allocation for part-time employee (0.5 FTE) = 0.25 FTE actual allocation
        /// </summary>
        /// <param name="projectAllocationPercentage">The project allocation percentage (0-100)</param>
        /// <param name="employmentType">The employment type</param>
        /// <returns>The actual FTE allocation</returns>
        float CalculateEffectiveAllocation(float projectAllocationPercentage, EmploymentType employmentType);
    }
}