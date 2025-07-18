using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Dtos.Requests.Departments
{
    public class UpdateDepartmentRequestDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}
