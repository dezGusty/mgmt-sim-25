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

        public int? Page { get; set; }
        public int? PageSize { get; set; }

        public QueryParams ToQueryParams()
        {
            return new QueryParams
            {
                SortBy = SortBy ?? "Id",
                SortDescending = SortDescending ?? false,
                Page = Page,
                PageSize = PageSize
            };
        }
    }
}
