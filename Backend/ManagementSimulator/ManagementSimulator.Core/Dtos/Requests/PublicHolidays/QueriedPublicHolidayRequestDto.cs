using ManagementSimulator.Core.Dtos.Requests.PagedQueryParams;

namespace ManagementSimulator.Core.Dtos.Requests.PublicHolidays
{
    public class QueriedPublicHolidayRequestDto
    {
        public int? Year { get; set; }
        public QueryParamsDto PagedQueryParams { get; set; } = new QueryParamsDto();
    }
}