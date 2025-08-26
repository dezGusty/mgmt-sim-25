using System;
using System.Collections.Generic;
using ManagementSimulator.Core.Dtos.Responses;

namespace ManagementSimulator.Core.Dtos.Responses.User
{
    public class HrUserResponseDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public List<string> Roles { get; set; } = new List<string>();
        public int JobTitleId { get; set; }
        public string JobTitleName { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime DateOfEmployment { get; set; }
        public int Vacation { get; set; }

        public int TotalLeaveDays { get; set; }
        public int UsedLeaveDays { get; set; }
        public int RemainingLeaveDays { get; set; }
        public List<LeaveTypeStatDto> LeaveTypeStatistics { get; set; } = new List<LeaveTypeStatDto>();
    }
}