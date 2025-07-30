using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Dtos.Requests.EmployeeManagers
{
    public class CreateManagersForEmployeeRequest
    {
        public int EmployeeId { get; set; }
        public List<int> ManagersIds { get; set; } = new List<int>();
    }

    public class PatchManagersForEmployeeRequest
    {
        public int EmployeeId { get; set; }
        public List<int>? ManagersIds { get; set; }
    }
}
