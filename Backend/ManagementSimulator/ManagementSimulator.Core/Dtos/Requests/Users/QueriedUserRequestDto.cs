using ManagementSimulator.Core.Dtos.Requests.PagedQueryParams;
using ManagementSimulator.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Dtos.Requests.Users
{
    public class QueriedUserRequestDto
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Department { get; set; }
        public string? JobTitle { get; set; }
        public string? GlobalSearch { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeEmail { get; set; }
        public string? ManagerName { get; set; }
        public string? ManagerEmail { get; set; }
        public string? UnassignedName { get; set; }
        public UserActivityStatus? ActivityStatus { get; set; } 
        public QueryParamsDto PagedQueryParams { get; set; } = new QueryParamsDto();
    }
}
