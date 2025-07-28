using ManagementSimulator.Database.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Dtos.Responses.User
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string? Email { get; set; } = string.Empty;
        public string? FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; } = string.Empty;
        public List<string>? Roles { get; set; }
        public int? JobTitleId { get; set; }
        public string? JobTitleName { get; set; } = string.Empty;
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; } = string.Empty;
        public List<int>? SubordinatesId { get; set; } = new List<int>();
        public List<string>? SubordinatesNames { get; set; } = new List<string>();
        public List<string>? SubordinatesJobTitles { get; set; } = new List<string>();
        public List<int>? SubordinatesJobTitleIds { get; set; } = new List<int>();
        public List<int>? ManagersIds { get; set; } = new List<int>();
        public List<string>? SubordinatesEmails { get; set; } = new List<string>();
        public int? EmployeeCount { get; set; } = 0;
        public bool? IsActive { get; set; } = true;
    }
}