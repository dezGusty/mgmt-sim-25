using System.ComponentModel.DataAnnotations;

namespace ManagementSimulator.Core.Dtos.Requests.WeekendConfiguration
{
    public class UpdateWeekendConfigurationRequestDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least one weekend day must be specified")]
        [MaxLength(7, ErrorMessage = "Cannot have more than 7 weekend days")]
        public List<string> WeekendDays { get; set; } = new List<string>();

        [Required]
        [Range(1, 7, ErrorMessage = "Weekend days count must be between 1 and 7")]
        public int WeekendDaysCount { get; set; }
        public bool IsValid()
        {
            if (WeekendDays.Count != WeekendDaysCount)
                return false;

            foreach (var dayName in WeekendDays)
            {
                if (!Enum.TryParse(dayName, true, out DayOfWeek _))
                    return false;
            }

            return WeekendDays.Count > 0 && WeekendDays.Count <= 7;
        }
    }
}
