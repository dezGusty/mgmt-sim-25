using ManagementSimulator.Core.Dtos.Requests.PagedQueryParams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Dtos.Requests.Users
{
    public class QueriedUserRequestDto
    {
        public string? LastName { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        public string? Department { get; set; } = string.Empty;
        public string? JobTitle { get; set; } = string.Empty;
        public string? GlobalSearch { get; set; } = string.Empty;
        public QueryParamsDto PagedQueryParams { get; set; } = new QueryParamsDto();
    }
}
