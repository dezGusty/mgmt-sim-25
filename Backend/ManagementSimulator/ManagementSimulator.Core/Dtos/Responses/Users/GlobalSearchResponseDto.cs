using ManagementSimulator.Core.Dtos.Responses.PagedResponse;
using ManagementSimulator.Core.Dtos.Responses.User;
using ManagementSimulator.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Dtos.Responses.Users
{
    public class GlobalSearchResponseDto
    {
        public PagedResponseDto<UserResponseDto>? Managers { get; set; }
        public PagedResponseDto<UserResponseDto>? Admins { get; set; }
        public PagedResponseDto<UserResponseDto>? UnassignedUsers { get; set; }
        public GlobalSearchCountsDto? TotalCounts { get; set; }
    }

    public class GlobalSearchCountsDto
    {
        public int TotalAdmins { get; set; }
        public int TotalManagers { get; set; }
        public int TotalUnassignedUsers { get; set; }
    }
}
