using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Dtos.Requests.LeaveRequestType
{
    public class UpdateLeaveRequestTypeRequestDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int? MaxDays { get; set; }
    }
}
