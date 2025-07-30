using ManagementSimulator.Database.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Database.Entities
{
    public class LeaveRequest : BaseEntity
    {
        // navigation properties
        public int UserId { get; set; }
        public User User { get; set; }

        public int? ReviewerId { get; set; }
        public User? Reviewer { get; set; }

        public int LeaveRequestTypeId { get; set; }
        public LeaveRequestType LeaveRequestType { get; set; }

        // fields
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string? Reason { get; set; }

        public RequestStatus RequestStatus { get; set; }

        [MaxLength(100)]
        public string? ReviewerComment { get; set; }
    }
}

