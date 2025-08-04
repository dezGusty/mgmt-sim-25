using ManagementSimulator.Core.Dtos.Requests.PagedQueryParams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Dtos.Requests.Departments
{
    public class QueriedDepartmentRequestDto
    {
        public string? Name { get; set; }
        public bool? IncludeDeleted { get; set; }
        public QueryParamsDto PagedQueryParams { get; set; } = new QueryParamsDto();
    }
}
