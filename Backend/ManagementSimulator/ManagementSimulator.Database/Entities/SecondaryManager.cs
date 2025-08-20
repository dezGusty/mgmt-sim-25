using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace ManagementSimulator.Database.Entities
{
    [PrimaryKey(nameof(EmployeeId), nameof(ManagerId), nameof(StartDate))]
    public class SecondaryManager
    {
        public int EmployeeId { get; set; }
        public User Employee { get; set; }

        public int ManagerId { get; set; }
        public User Manager { get; set; }

        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }

        public bool IsActive => DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate;
    }
}