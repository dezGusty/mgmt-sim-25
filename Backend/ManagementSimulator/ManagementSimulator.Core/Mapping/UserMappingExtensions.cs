using ManagementSimulator.Core.Dtos.Responses;
using ManagementSimulator.Core.Dtos.Responses.User;
using ManagementSimulator.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Mapping
{
    public static class UserMappingExtensions
    {
        public static UserResponseDto ToUserResponseDto(this User user)
        {
            return new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = user.Roles
                    .Where(er => er.DeletedAt == null && er.Role != null)
                    .Select(er => er.Role.Rolename)
                    .ToList(),
                JobTitleId = user.JobTitleId,
                JobTitleName = user.Title.Name,
                Vacation = user.Vacation,
                EmploymentType = user.EmploymentType,
                TotalAvailability = user.TotalAvailability,
                RemainingAvailability = user.RemainingAvailability
            };
        }
    }
}
