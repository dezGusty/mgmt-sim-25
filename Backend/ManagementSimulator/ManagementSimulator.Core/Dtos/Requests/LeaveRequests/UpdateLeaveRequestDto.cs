using ManagementSimulator.Database.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Dtos.Requests.LeaveRequests
{
    public class UpdateLeaveRequestDto
    {
        public int? UserId { get; set; }
        public int? ReviewerId { get; set; }
        public int? LeaveRequestTypeId { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string? Reason { get; set; } = string.Empty;
        public bool? IsApproved { get; set; }
        public RequestStatus? RequestStatus { get; set; }

        public string? ReviewerComment { get; set; } = string.Empty;
    }
}
