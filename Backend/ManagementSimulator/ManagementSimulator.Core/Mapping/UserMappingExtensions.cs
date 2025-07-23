using ManagementSimulator.Core.Dtos.Responses.User;
using ManagementSimulator.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
                Role = string.Join(" ", user.Roles.Where(eru => eru.DeletedAt == null).Select(ru => ru.Role.Rolename)),
                JobTitleId = user.JobTitleId,
                JobTitleName = user.Title.Name
            };
        }

        public static CreateUserResponseDto ToCreateUserResponseDto(this User user)
        {
            return new CreateUserResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                JobTitleId = user.JobTitleId,
                JobTitleName = user.Title.Name,
                DepartmentId = user.Title.DepartmentId,
                DepartmentName = user.Title.Department.Name
            };
        }
    }
}
