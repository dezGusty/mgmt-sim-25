using ManagementSimulator.Core.Dtos.Requests.PagedQueryParams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Dtos.Requests.LeaveRequestTypes
{
    public class QueriedLeaveRequestTypeRequestDto
    {
        public string? Title { get; set; } = string.Empty;
        public QueryParamsDto PagedQueryParams { get; set; } = new QueryParamsDto();
    }
}
