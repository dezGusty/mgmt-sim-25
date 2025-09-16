using ManagementSimulator.Core.Dtos.Requests.PublicHolidays;
using ManagementSimulator.Core.Dtos.Responses.PagedResponse;
using ManagementSimulator.Core.Dtos.Responses.PublicHolidays;

namespace ManagementSimulator.Core.Services.Interfaces
{
    public interface IPublicHolidayService
    {
        Task<List<PublicHolidayResponseDto>> GetHolidaysByYearAsync(int year);
        Task<PublicHolidayResponseDto?> GetHolidayByIdAsync(int id);
        Task<PublicHolidayResponseDto> CreateHolidayAsync(CreatePublicHolidayRequestDto request);
        Task<PublicHolidayResponseDto?> UpdateHolidayAsync(int id, UpdatePublicHolidayRequestDto request);
        Task<bool> DeleteHolidayAsync(int id);
        Task<bool> HardDeleteHolidayAsync(int id);
        Task<PagedResponseDto<PublicHolidayResponseDto>> GetAllHolidaysFilteredAsync(QueriedPublicHolidayRequestDto request);
        Task<List<PublicHolidayResponseDto>> GetHolidaysInRangeAsync(DateTime startDate, DateTime endDate);
    }
}