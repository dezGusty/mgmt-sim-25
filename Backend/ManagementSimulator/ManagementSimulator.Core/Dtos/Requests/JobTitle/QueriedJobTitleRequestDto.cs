using ManagementSimulator.Core.Dtos.Requests.PagedQueryParams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Dtos.Requests.JobTitle
{
    public class QueriedJobTitleRequestDto
    {
        public string? JobTitleName { get; set; } = string.Empty;
        public string? DepartmentName { get; set; } = string.Empty;

        public QueryParamsDto PagedQueryParams { get; set; } = new QueryParamsDto();
    }
}
