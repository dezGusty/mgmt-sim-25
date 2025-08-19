using System;
using System.ComponentModel.DataAnnotations;

namespace ManagementSimulator.Core.Dtos.Requests.SecondaryManagers
{
    public class UpdateSecondaryManagerRequest
    {
        [Required]
        public DateTime NewEndDate { get; set; }

        public string? Reason { get; set; }
    }
}