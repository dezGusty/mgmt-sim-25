using ManagementSimulator.Database.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ManagementSimulator.Database.Entities
{
    public class User : BaseEntity
    {
        // navigation properties
        public int JobTitleId { get; set; }
        public JobTitle Title { get; set; }
        public int DepartmentId { get; set; }
        public Department Department { get; set; }

        public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
        public ICollection<LeaveRequest> ReviewedRequests { get; set; } = new List<LeaveRequest>();

        public ICollection<EmployeeManager> Managers { get; set; } = new List<EmployeeManager>(); 
        public ICollection<EmployeeManager> Subordinates { get; set; } = new List<EmployeeManager>();

        public ICollection<EmployeeRoleUser> Roles { get; set; } = new List<EmployeeRoleUser>();

        // fields
        [Required,MaxLength(50),EmailAddress]
        public string Email { get; set; }

        [Required, MaxLength(50)]
        public string FirstName { get; set; }

        [Required, MaxLength(50)]
        public string LastName { get; set; }

        public string PasswordHash { get; set; }
        public bool MustChangePassword { get; set; }
        public DateTime DateOfEmployment { get; set; }
        public string FullName => $"{FirstName} {LastName}";
    }
}
