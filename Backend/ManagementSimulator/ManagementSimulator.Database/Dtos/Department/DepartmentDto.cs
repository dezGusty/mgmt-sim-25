using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Database.Dtos.Department
{
    public class DepartmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int EmployeeCount { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
