using System.ComponentModel.DataAnnotations;

namespace ManagementSimulator.Core.Dtos.Requests.PublicHolidays
{
    public class UpdatePublicHolidayRequestDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }

        public bool IsRecurring { get; set; } = false;
    }
}