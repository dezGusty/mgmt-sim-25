using ManagementSimulator.Core.Dtos.Requests.PagedQueryParams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Dtos.Requests.Users
{
    public class GlobalSearchRequestDto
    {
        public string? GlobalSearch { get; set; }
        public string? SearchCategory { get; set; }
        
        public QueryParamsDto ManagersPagedParams { get; set; } = new QueryParamsDto();
          
        public QueryParamsDto UnassignedUsersPagedParams { get; set; } = new QueryParamsDto();
        
        public QueryParamsDto AdminsPagedParams { get; set; } = new QueryParamsDto();
        
        public bool IncludeTotalCounts { get; set; } = true;
    }
}
