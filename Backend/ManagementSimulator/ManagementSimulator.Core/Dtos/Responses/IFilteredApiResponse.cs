namespace ManagementSimulator.Core.Dtos.Responses
{
    public interface IFilteredApiResponse<T>
    {
        IEnumerable<T> Data { get; set; }
        int TotalCount { get; set; }
        int PageNumber { get; set; }
        int PageSize { get; set; }
        int TotalPages { get; set; }
    }
}