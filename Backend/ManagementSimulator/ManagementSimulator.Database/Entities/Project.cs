using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ManagementSimulator.Database.Entities
{
    public class Project : BaseEntity
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public float BudgetedFTEs { get; set; }

        public bool IsActive { get; set; }

        public ICollection<UserProject> UserProjects { get; set; } = new List<UserProject>();
    }
}