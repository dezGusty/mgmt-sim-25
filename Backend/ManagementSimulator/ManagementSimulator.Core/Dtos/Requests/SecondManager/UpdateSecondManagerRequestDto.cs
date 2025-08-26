using System.ComponentModel.DataAnnotations;

namespace ManagementSimulator.Core.Dtos.Requests.SecondManager
{
    public class UpdateSecondManagerRequestDto
    {
        [Required]
        public DateTime NewEndDate { get; set; }
    }
} 