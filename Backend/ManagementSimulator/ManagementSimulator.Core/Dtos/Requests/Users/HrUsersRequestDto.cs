using System;

namespace ManagementSimulator.Core.Dtos.Requests.Users
{
    public class HrUsersRequestDto
    {
        public int? Year { get; set; } = DateTime.Now.Year;
        public int? Page { get; set; } = 1;
        public int? PageSize { get; set; } = 10; 
        public string? Department { get; set; } 
    }
}