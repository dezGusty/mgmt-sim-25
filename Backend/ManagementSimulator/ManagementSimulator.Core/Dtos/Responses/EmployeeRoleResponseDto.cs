using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Dtos.Responses
{
    public class EmployeeRoleResponseDto
    {
        public int Id { get; set; }
        public string? Rolename { get; set; } = string.Empty;
    }
}
