using System;
using System.Collections.Generic;
using System.Linq;

namespace ManagementSimulator.Infrastructure.Config
{
    public class WeekendConfiguration
    {
        public List<string> WeekendDays { get; set; } = new List<string>();
        public int WeekendDaysCount { get; set; } = 0;
        public List<DayOfWeek> GetWeekendDaysOfWeek()
        {
            var weekendDaysOfWeek = new List<DayOfWeek>();
            
            foreach (var dayName in WeekendDays)
            {
                if (Enum.TryParse(dayName, true, out DayOfWeek dayOfWeek))
                {
                    weekendDaysOfWeek.Add(dayOfWeek);
                }
            }
            
            return weekendDaysOfWeek;
        }
        public bool IsWeekend(DateTime date)
        {
            var weekendDaysOfWeek = GetWeekendDaysOfWeek();
            return weekendDaysOfWeek.Contains(date.DayOfWeek);
        }
        public bool IsValid()
        {
            foreach (var dayName in WeekendDays)
            {
                if (!Enum.TryParse(dayName, true, out DayOfWeek _))
                {
                    return false;
                }
            }

            return WeekendDaysCount == WeekendDays.Count && WeekendDaysCount >= 0 && WeekendDaysCount <= 7;
        }
    }
}
