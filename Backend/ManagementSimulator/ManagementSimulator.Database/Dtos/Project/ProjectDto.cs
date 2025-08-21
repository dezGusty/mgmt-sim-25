using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Database.Dtos.Project
{
    public class ProjectDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public float BudgetedFTEs { get; set; }
        public bool IsActive { get; set; }
        public int AssignedUsersCount { get; set; }
        public float TotalAssignedPercentage { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}