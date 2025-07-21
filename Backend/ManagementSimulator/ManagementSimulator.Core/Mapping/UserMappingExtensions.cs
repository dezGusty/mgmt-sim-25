using ManagementSimulator.Core.Dtos.Responses;
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
                Role = string.Join(" ", user.Roles.Select(r => r.Rolename)),
                JobTitleId = user.JobTitleId,
                JobTitleName = user.Title?.Name ?? string.Empty
            };
        }
    }
}
