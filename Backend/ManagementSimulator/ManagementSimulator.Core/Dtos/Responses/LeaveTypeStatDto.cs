using System;

namespace ManagementSimulator.Core.Dtos.Responses
{
    public class LeaveTypeStatDto
    {
        public int LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;
        public int UsedDays { get; set; }
        public int RemainingDays { get; set; }
        public int MaxAllowedDays { get; set; }
    }
} 