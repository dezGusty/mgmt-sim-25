namespace ManagementSimulator.Core.Dtos.Responses.WeekendConfiguration
{
    public class WeekendConfigurationResponseDto
    {
        public List<string> WeekendDays { get; set; } = new List<string>();
        public int WeekendDaysCount { get; set; }
        public bool IsValid { get; set; }
        public DateTime LastUpdated { get; set; }
        public List<string> AvailableDays { get; set; } = new List<string>
        {
            "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"
        };
    }
}
