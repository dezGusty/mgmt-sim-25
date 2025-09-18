using ManagementSimulator.Core.Services.Interfaces;
using ManagementSimulator.Infrastructure.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ManagementSimulator.Core.Services
{
    public class WeekendService : IWeekendService
    {
        private WeekendConfiguration GetCurrentConfiguration()
        {
            return AppConfig.WeekendSettings ?? new WeekendConfiguration
            {
                WeekendDays = new List<string> { "Saturday", "Sunday" },
                WeekendDaysCount = 2
            };
        }

        public bool IsWeekend(DateTime date)
        {
            return GetCurrentConfiguration().IsWeekend(date);
        }

        public int GetWeekendDaysCount()
        {
            return GetCurrentConfiguration().WeekendDaysCount;
        }

        public List<DayOfWeek> GetWeekendDays()
        {
            return GetCurrentConfiguration().GetWeekendDaysOfWeek();
        }

        public int CountWorkingDays(DateTime startDate, DateTime endDate, HashSet<DateTime> publicHolidays)
        {
            int workingDays = 0;
            DateTime currentDate = startDate.Date;
            DateTime endDateOnly = endDate.Date;

            while (currentDate <= endDateOnly)
            {
                if (!IsWeekend(currentDate) && !publicHolidays.Contains(currentDate))
                {
                    workingDays++;
                }
                currentDate = currentDate.AddDays(1);
            }

            return workingDays;
        }

        public int CountWeekendDays(DateTime startDate, DateTime endDate)
        {
            int weekendDays = 0;
            DateTime currentDate = startDate.Date;
            DateTime endDateOnly = endDate.Date;

            while (currentDate <= endDateOnly)
            {
                if (IsWeekend(currentDate))
                {
                    weekendDays++;
                }
                currentDate = currentDate.AddDays(1);
            }

            return weekendDays;
        }

        public WeekendConfiguration GetWeekendConfiguration()
        {
            return GetCurrentConfiguration();
        }

        public bool UpdateWeekendConfiguration(List<string> weekendDays, int weekendDaysCount)
        {
            var tempConfig = new WeekendConfiguration
            {
                WeekendDays = weekendDays,
                WeekendDaysCount = weekendDaysCount
            };

            if (!tempConfig.IsValid())
            {
                return false;
            }

            if (AppConfig.WeekendSettings != null)
            {
                AppConfig.WeekendSettings.WeekendDays = weekendDays;
                AppConfig.WeekendSettings.WeekendDaysCount = weekendDaysCount;
            }

            return true;
        }

        public WeekendConfiguration GetUpdatedConfiguration(List<string> weekendDays, int weekendDaysCount)
        {
            return new WeekendConfiguration
            {
                WeekendDays = weekendDays,
                WeekendDaysCount = weekendDaysCount
            };
        }
    }
}
