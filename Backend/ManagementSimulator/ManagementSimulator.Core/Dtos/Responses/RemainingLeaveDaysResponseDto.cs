using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Dtos.Responses
{
    public class RemainingLeaveDaysResponseDto
    {
        public int UserId { get; set; }
        public int LeaveRequestTypeId { get; set; }
        public string LeaveRequestTypeName { get; set; } = string.Empty;
        public int? MaxDaysAllowed { get; set; }
        public int DaysUsed { get; set; }
        public int? RemainingDays { get; set; }
        public bool HasUnlimitedDays { get; set; }
        public List<LeaveRequestSummaryDto> UsedLeaveRequests { get; set; } = new List<LeaveRequestSummaryDto>();
    }

    public class LeaveRequestSummaryDto
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DaysCount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
