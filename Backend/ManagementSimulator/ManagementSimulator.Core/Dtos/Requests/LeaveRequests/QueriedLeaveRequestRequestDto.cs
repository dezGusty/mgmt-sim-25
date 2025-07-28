using ManagementSimulator.Core.Dtos.Requests.PagedQueryParams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Dtos.Requests.LeaveRequests
{
    public class QueriedLeaveRequestRequestDto
    {
        public string? Email {  get; set; }
        public string? LastName { get; set; }   
        public QueryParamsDto PagedQueryParams { get; set; } = new QueryParamsDto();
    }
}
