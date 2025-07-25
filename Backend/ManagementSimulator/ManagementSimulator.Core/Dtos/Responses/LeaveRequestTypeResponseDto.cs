﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Dtos.Responses
{
    public class LeaveRequestTypeResponseDto
    {
        public int Id { get; set; }
        public string? Description { get; set; } = string.Empty;
        public string? AdditionalDetails { get; set; }
    }
}
