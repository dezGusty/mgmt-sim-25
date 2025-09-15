using ManagementSimulator.Database.Entities;

namespace ManagementSimulator.Database.Repositories.Intefaces
{
    public interface IPublicHolidayRepository : IBaseRepostory<PublicHoliday>
    {
        Task<List<PublicHoliday>> GetHolidaysByYearAsync(int year, bool includeDeleted = false);
        Task<PublicHoliday?> GetHolidayByNameAndDateAsync(string name, DateTime date, bool includeDeleted = false);
        Task<List<PublicHoliday>> GetHolidaysInRangeAsync(DateTime startDate, DateTime endDate, bool includeDeleted = false);
    }
}