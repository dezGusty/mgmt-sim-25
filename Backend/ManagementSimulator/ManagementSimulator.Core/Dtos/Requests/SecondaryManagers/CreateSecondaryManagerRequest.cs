using System;
using System.ComponentModel.DataAnnotations;

namespace ManagementSimulator.Core.Dtos.Requests.SecondaryManagers
{
    public class CreateSecondaryManagerRequest
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public int SecondaryManagerId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public string? Reason { get; set; }
    }
}