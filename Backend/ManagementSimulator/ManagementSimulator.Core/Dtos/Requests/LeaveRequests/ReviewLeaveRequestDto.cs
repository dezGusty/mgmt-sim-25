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
        public RequestStatus RequestStatus { get; set; }
        public string ReviewerComment { get; set; } = string.Empty;


    }

}
