using ManagementSimulator.Database.Dtos.QueryParams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Dtos.Requests.PagedQueryParams
{
    public class QueryParamsDto
    {
        public string? SortBy { get; set; } = "Id";
        public bool? SortDescending { get; set; } = false;

        public int? Page { get; set; } = 1;
        public int? PageSize { get; set; } = 1;

        public QueryParams ToQueryParams()
        {
            return new QueryParams
            {
                SortBy = SortBy ?? "Id",
                SortDescending = SortDescending ?? false,
                Page = Page ?? 1,
                PageSize = PageSize ?? 1
            };
        }
    }
}
