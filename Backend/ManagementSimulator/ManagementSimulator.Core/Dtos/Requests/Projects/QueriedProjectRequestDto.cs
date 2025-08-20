using ManagementSimulator.Core.Dtos.Requests.PagedQueryParams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Dtos.Requests.Projects
{
    public class QueriedProjectRequestDto
    {
        public string? Name { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public DateTime? EndDateFrom { get; set; }
        public DateTime? EndDateTo { get; set; }
        public QueryParamsDto PagedQueryParams { get; set; } = new QueryParamsDto();
    }
}