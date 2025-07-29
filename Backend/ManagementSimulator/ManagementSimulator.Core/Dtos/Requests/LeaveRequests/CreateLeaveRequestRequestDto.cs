using ManagementSimulator.Database.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Dtos.Requests.LeaveRequest
{
    public class CreateLeaveRequestRequestDto
    {
        public int UserId { get; set; }
        public int LeaveRequestTypeId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string? Reason { get; set; } = string.Empty;
    }
}
