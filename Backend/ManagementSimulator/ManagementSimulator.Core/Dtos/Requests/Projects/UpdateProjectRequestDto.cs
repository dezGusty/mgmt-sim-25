using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Dtos.Requests.Projects
{
    public class UpdateProjectRequestDto
    {
        public string? Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public float? BudgetedFTEs { get; set; }
        public bool? IsActive { get; set; }
    }
}