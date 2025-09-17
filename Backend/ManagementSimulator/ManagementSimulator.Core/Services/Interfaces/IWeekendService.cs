using System;

namespace ManagementSimulator.Core.Services.Interfaces
{
    public interface IWeekendService
    {
        bool IsWeekend(DateTime date);

        int GetWeekendDaysCount();

        System.Collections.Generic.List<DayOfWeek> GetWeekendDays();

        int CountWorkingDays(DateTime startDate, DateTime endDate, System.Collections.Generic.HashSet<DateTime> publicHolidays);

        int CountWeekendDays(DateTime startDate, DateTime endDate);

        ManagementSimulator.Infrastructure.Config.WeekendConfiguration GetWeekendConfiguration();

        bool UpdateWeekendConfiguration(System.Collections.Generic.List<string> weekendDays, int weekendDaysCount);

        ManagementSimulator.Infrastructure.Config.WeekendConfiguration GetUpdatedConfiguration(System.Collections.Generic.List<string> weekendDays, int weekendDaysCount);
    }
}
