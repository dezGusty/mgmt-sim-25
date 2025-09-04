using ManagementSimulator.Database.Context;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Repositories.Intefaces;
using Microsoft.EntityFrameworkCore;

namespace ManagementSimulator.Database.Repositories
{
    public class PublicHolidayRepository : BaseRepository<PublicHoliday>, IPublicHolidayRepository
    {
        public PublicHolidayRepository(MGMTSimulatorDbContext databaseContext) : base(databaseContext)
        {
        }

        public async Task<List<PublicHoliday>> GetHolidaysByYearAsync(int year, bool includeDeleted = false)
        {
            return await GetRecords(includeDeleted)
                .Where(h => h.Date.Year == year)
                .OrderBy(h => h.Date)
                .ToListAsync();
        }

        public async Task<PublicHoliday?> GetHolidayByNameAndDateAsync(string name, DateTime date, bool includeDeleted = false)
        {
            return await GetRecords(includeDeleted)
                .FirstOrDefaultAsync(h => h.Name.ToLower() == name.ToLower() && 
                                         h.Date.Date == date.Date);
        }
    }
}