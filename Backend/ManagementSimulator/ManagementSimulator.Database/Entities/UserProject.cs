using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Database.Entities
{
    public class UserProject
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        public float TimePercentagePerProject { get; set; }
    }
}