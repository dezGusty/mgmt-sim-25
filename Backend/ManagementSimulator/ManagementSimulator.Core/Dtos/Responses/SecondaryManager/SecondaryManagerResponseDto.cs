using ManagementSimulator.Core.Dtos.Responses.User;
using System;

namespace ManagementSimulator.Core.Dtos.Responses.SecondaryManager
{
    public class SecondaryManagerResponseDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public int SecondaryManagerId { get; set; }
        public string SecondaryManagerName { get; set; } = string.Empty;
        public int AssignedByAdminId { get; set; }
        public string AssignedByAdminName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Reason { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}