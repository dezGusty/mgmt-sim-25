using System.ComponentModel.DataAnnotations;

namespace ManagementSimulator.Core.Dtos.Requests.SecondManager
{
    public class CreateSecondManagerRequestDto
    {
        [Required]
        public int SecondManagerEmployeeId { get; set; }

        [Required]
        public int ReplacedManagerId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }
    }
} 