using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Dtos.Responses
{
    public class UserProjectResponseDto
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public string? JobTitleName { get; set; }
        public string? EmploymentType { get; set; }
        public int ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public float TimePercentagePerProject { get; set; }
    }
}