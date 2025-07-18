using ManagementSimulator.Database.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Dtos.Requests.LeaveRequest
{
    public class ReviewLeaveRequestDto
    {
        public int Id { get; set; }
        public bool IsApproved { get; set; }
        public RequestStatus RequestStatus { get; set; }
        public string ReviewerComment { get; set; } = string.Empty;
    }

}
