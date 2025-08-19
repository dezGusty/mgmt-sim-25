using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace ManagementSimulator.Database.Entities
{
    [PrimaryKey(nameof(EmployeeId), nameof(SecondaryManagerId), nameof(StartDate))]
    public class SecondaryManager
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        
        public int EmployeeId { get; set; }
        public User Employee { get; set; }

        public int SecondaryManagerId { get; set; }
        public User Manager { get; set; }

        public int AssignedByAdminId { get; set; }
        public User AssignedByAdmin { get; set; }

        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }

        public string? Reason { get; set; }

        public bool IsActive => DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate && DeletedAt == null;
    }
}