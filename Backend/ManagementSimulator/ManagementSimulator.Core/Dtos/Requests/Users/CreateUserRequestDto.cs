using ManagementSimulator.Database.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Dtos.Requests.Users
{
    public class CreateUserRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
        public int JobTitleId { get; set; }
        public List<int> EmployeeRolesId { get; set; } = new List<int>();
        public DateTime DateOfEmployment { get; set; }
        public int? Vacation { get; set; }
        public EmploymentType EmploymentType { get; set; } = EmploymentType.FullTime;
    }
}
