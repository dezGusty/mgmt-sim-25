using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Dtos.Requests.Projects
{
    public class AssignUserToProjectRequestDto
    {
        public int UserId { get; set; }
        public float TimePercentagePerProject { get; set; }
    }
}