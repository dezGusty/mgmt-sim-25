using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Dtos.Responses.User
{
    public class CreateUserResponseDto
    {
        public int Id { get; set; }
        public string? Email { get; set; } = string.Empty;
        public string? FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; } = string.Empty;
        public int? JobTitleId { get; set; }
        public string? JobTitleName { get; set; } = string.Empty;
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; } = string.Empty;
        public int? AnnuallyLeaveDays { get; set; }
        public int? LeaveDaysLeftCurrentYear { get; set; }
        public DateTime? DateOfEmployment { get; set; }
    }
}
