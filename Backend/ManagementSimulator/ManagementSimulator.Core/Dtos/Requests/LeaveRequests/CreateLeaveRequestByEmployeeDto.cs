using System;

namespace ManagementSimulator.Core.Dtos.Requests.LeaveRequests
{
    public class CreateLeaveRequestByEmployeeDto
    {
        public int LeaveRequestTypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}