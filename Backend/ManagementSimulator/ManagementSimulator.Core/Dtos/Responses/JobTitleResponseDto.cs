using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Dtos.Responses
{
    public class JobTitleResponseDto
    {
        public int Id { get; set; }
        public string? Name { get; set; } = string.Empty;
        public int? EmployeeCount { get; set; } = 0;
    }
}
