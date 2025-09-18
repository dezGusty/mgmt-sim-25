using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Database.Entities
{
    public class LeaveRequestType : BaseEntity
    {
        // navigation properties
        public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();

        // fields
        [MaxLength(50)]
        public string Title { get; set; } = null!;

        [MaxLength(100)]
        public string? Description { get; set; }

        public int? MaxDays { get; set; }

        public bool IsPaid { get; set; } = false;
    }
}
