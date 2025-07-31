using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Dtos.Requests.LeaveRequestType
{
    public class CreateLeaveRequestTypeRequestDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? MaxDays { get; set; }
        public bool IsPaid { get; set; } = false;
    }
}
