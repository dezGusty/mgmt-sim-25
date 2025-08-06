using ManagementSimulator.Database.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Dtos.Responses.LeaveRequest
{
    public class CreateLeaveRequestResponseDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int LeaveRequestTypeId { get; set; }
        public string LeaveRequestTypeName { get; set; } = string.Empty;
        public bool LeaveRequestTypeIsPaid { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public RequestStatus RequestStatus { get; set; }
        public string ReviewerComment { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
    }
}
