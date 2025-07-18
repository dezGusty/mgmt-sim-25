using ManagementSimulator.Database.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Dtos.Responses
{
    public class LeaveRequestResponseDto
    {
        public int Id { get; set; } 
        public int UserId { get; set; }

        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;

        public int? ReviewerId { get; set; }
        public string? ReviewerName { get; set; }

        public int LeaveRequestTypeId { get; set; }
        public string LeaveRequestTypeName { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string Reason { get; set; } = string.Empty;
        public bool IsApproved { get; set; }

        public RequestStatus RequestStatus { get; set; }

        public string ReviewerComment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

    }
}
