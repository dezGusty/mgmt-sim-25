using System.ComponentModel.DataAnnotations;

namespace ManagementSimulator.Database.Entities
{
    public class PublicHoliday : BaseEntity
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }

        public bool IsRecurring { get; set; } = false;
    }
}