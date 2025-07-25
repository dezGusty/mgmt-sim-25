﻿using System;
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
        [MaxLength(100)]
        public string AdditionalDetails { get; set; }
        [MaxLength(50)]
        public string Description { get; set; } 
    }
}
